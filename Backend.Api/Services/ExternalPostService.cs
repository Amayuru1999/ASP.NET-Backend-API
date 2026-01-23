namespace Backen.Api.Services;

public class ExternalPostService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public ExternalPostService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<ExternalPostDto>> GetAllSync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("JsonPlaceHolder");
        using var response = await client.GetAsync("/posts", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalApiException("❗Failed to fetch posts from external API: ",response.StatusCode);
        }
        
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var posts = await JsonSerializer.DeserializeAsync<List<ExternalPostDto>>(stream, _serializerOptions,
                        cancellationToken)
                    ?? new List<ExternalPostDto>();
        return posts;
    }
}