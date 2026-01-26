using System.Net;
using System.Text.Json;
using Backend.Api;
using Backend.Api.Dtos;


namespace Backend.Api.Services;

public class ExternalPostService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public ExternalPostService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<ExternalPostDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(Constants.JsonPlaceholderClientName);
        using var response = await client.GetAsync(Constants.PostsEndpoint, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalApiException("❗Failed to fetch posts from external API.", response.StatusCode);
        }
        
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var posts = await JsonSerializer.DeserializeAsync<List<ExternalPostDto>>(stream, _serializerOptions,
                        cancellationToken)
                    ?? new List<ExternalPostDto>();
        return posts;
    }

    public async Task<ExternalPostDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(Constants.JsonPlaceholderClientName);
        var endpoint = Constants.PostByIdEndpoint.Replace("{id}", id.ToString());
        using var response = await client.GetAsync(endpoint, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalApiException("❗Failed to fetch post from external API.", response.StatusCode);
        }
        
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var post = await JsonSerializer.DeserializeAsync<ExternalPostDto>(stream, _serializerOptions,
            cancellationToken);
        return post;
    }

    public async Task<bool> PingAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(Constants.JsonPlaceholderClientName);
        using var response = await client.GetAsync("/posts/1", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public sealed class ExternalApiException : Exception
    {
        public ExternalApiException(string message, HttpStatusCode statusCode) : base(message)
    	{
        StatusCode = statusCode;
    	}

        public HttpStatusCode StatusCode { get; }
    }

}
