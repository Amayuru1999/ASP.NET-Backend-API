namespace Backend.Api.Dtos;

public sealed class ExternalPostDto
{
    public int UserId {get; init;}
    public int Id {get; init;}
    public string? Title {get; init;}
    public string? Body {get; init;}
}


