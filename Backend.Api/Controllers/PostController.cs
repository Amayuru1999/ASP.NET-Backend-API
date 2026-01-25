using Backend.Api.Data;
using Backend.Api.Dtos;
using Backend.Api.Models;
using Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PostController : ControllerBase
{
    private readonly IPostRepository _repository;
    private readonly ExternalPostService  _externalPostService;

    public PostController(IPostRepository repository, ExternalPostService externalService)
    {
        _repository = repository;
        _externalPostService = externalService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PostResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var count = await _repository.GetCountAsync(cancellationToken);
            if (count == 0)
            {
                var externalPosts = await _externalPostService.GetAllAsync(cancellationToken);
                if (externalPosts.Count > 0)
                {
                    var records = externalPosts.Select(MapToRecord).ToList();
                    await _repository.InsertManyAsync(records, cancellationToken);
                }

                return Ok(externalPosts.Select(MapToResponse).ToList());
            }
        }
        catch (ExternalPostService.ExternalApiException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Database error occurred while fetching posts.",
                detail = ex.Message
            });
        }
        catch (SqlException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Database error occurred while fetching posts.",
                detail = ex.Message
            });
        }
    }

    [HttpGet("{id:int")]
    public async Task<ActionResult<PostResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var cached = await _repository.GetByIdAsync(id, cancellationToken);
            if (cached is not null)
            {
                
            }

        }
    }
    private static PostRecord MapToRecord(ExternalPostDto post)
    {
        return new PostRecord
        {
            Id = post.Id,
            UserId = post.UserId,
            Title = post.Title ?? string.Empty,
            Body = post.Body ?? string.Empty,
            FetchedAtUtc = DateTime.UtcNow
        };
    }
    private static PostResponseDto MapToResponse(ExternalPostDto post)
    {
        return new PostResponseDto
        {
            Id = post.Id,
            UserId = post.UserId,
            Title = post.Title ?? string.Empty,
            Body = post.Body ?? string.Empty
        };
    }
}