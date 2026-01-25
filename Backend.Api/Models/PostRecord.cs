namespace Backend.Api.Models;

public class PostRecord
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime FetchedAtUtc { get; init; }
}