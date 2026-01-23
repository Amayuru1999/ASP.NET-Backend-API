using Backend.Api.Models;
namespace Backend.Api.Data;

public interface IPostRepository
{
    Task<int> GetCountAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<PostRecord>> GetAllAsync(CancellationToken cancellationToken);
    Task<PostRecord?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task InsertAsync(PostRecord post, CancellationToken cancellationToken);
    Task InsertManyAsync(IEnumerable<PostRecord> posts, CancellationToken cancellationToken);
    
}