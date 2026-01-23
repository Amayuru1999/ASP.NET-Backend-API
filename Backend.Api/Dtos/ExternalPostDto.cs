namespace Backend.Api.Dtos;

public sealed class ExternalPostDto
{
    public int UserId {get; set;}
    public int Id {get; set;}
    public string? Title {get; set;}
    public string? Body {get; set;}
}


