using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient("JsonPlaceholder", client =>
{
    var baseUrl = builder.Configuration["ExternalApi:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    else
    {
        client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
    }
    client.Timeout = TimeSpan.FromSeconds(10);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    UseProxy = false,
    Proxy = null,
    ConnectCallback = async (context, cancellationToken) =>
    {
        var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host);
        var address = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? addresses.First();
        var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(address, context.DnsEndPoint.Port, cancellationToken);
        return new NetworkStream(socket, ownsSocket: true);
    }
});

builder.Services.AddScoped<Backend.Api.Data.IPostRepository, Backend.Api.Data.PostRepository>();
builder.Services.AddScoped<Backend.Api.Services.ExternalPostService>();
builder.Services.AddHostedService<Backend.Api.Services.PostWarmupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
