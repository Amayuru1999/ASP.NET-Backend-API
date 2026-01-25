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
    private readonly ExternalPostService _externalService;

    public PostController(IPostRepository repository, ExternalPostService externalService)
    {
        _repository = repository;
        _externalService = externalService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PostResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var count = await _repository.GetCountAsync(cancellationToken);
            if (count == 0)
            {
                var externalPosts = await _externalService.GetAllAsync(cancellationToken);
                if (externalPosts.Count > 0)
                {
                    var records = externalPosts.Select(MapToRecord).ToList();
                    await _repository.InsertManyAsync(records, cancellationToken);
                }

                return Ok(externalPosts.Select(MapToResponse).ToList());
            }
            var cached = await _repository.GetAllAsync(cancellationToken);
            return Ok(cached.Select(MapToResponse).ToList());
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

[HttpGet("{id:int}")]
    public async Task<ActionResult<PostResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var cached = await _repository.GetByIdAsync(id, cancellationToken);
            if (cached is not null)
            {
                return Ok(MapToResponse(cached));
            }

            var externalPost = await _externalService.GetByIdAsync(id, cancellationToken);
            if (externalPost is null)
            {
                return NotFound(new { message = $"Post {id} not found." });
            }

            var record = MapToRecord(externalPost);
            await _repository.InsertAsync(record, cancellationToken);
            return Ok(MapToResponse(record));
        }
        catch (ExternalPostService.ExternalApiException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = ex.Message,
                status = (int)ex.StatusCode
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
    private static PostResponseDto MapToResponse(PostRecord post)
    {
        return new PostResponseDto
        {
            Id = post.Id,
            UserId = post.UserId,
            Title = post.Title,
            Body = post.Body
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