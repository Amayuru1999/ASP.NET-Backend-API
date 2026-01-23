namespace Backend.Api.Dtos;

public sealed class ExternalPostDto
{
    public int UserId {get; set;}
    public int Id {get; init;}
    public string? Title {get; set;}
    public string? Body {get; set;}
}


