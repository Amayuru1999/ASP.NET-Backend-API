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
});

builder.Services.AddScoped<Backend.Api.Data.IPostRepository, Backend.Api.Data.PostRepository>();
builder.Services.AddScoped<Backend.Api.Services.ExternalPostService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();