using System.Data;
using Backend.Api.Models;
using Microsoft.Data.SqlClient;

namespace Backend.Api.Data
{
    public sealed class PostRepository : IPostRepository
    {
        private readonly string _connectionString;

        public PostRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("❗ConnectionString not set");
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Posts";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is int count ? count : Convert.ToInt32(result);
            
        }

        public async Task<IReadOnlyList<PostRecord>> GetAllAsync(CancellationToken cancellationToken)
        {
            const string sql = @"SELECT Id, Title, Body, FetchedAtUtc FROM dbo.Posts ORDER BY Id";
            
            var results = new List<PostRecord>();
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(Map(reader));
            }
            return results;
        }

        public async Task<PostRecord?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT Id, UserId, Title, Body, FetchedAtUtc FROM dbo.Posts WHERE Id = @Id";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }

        public async Task InsertAsync(PostRecord post, CancellationToken cancellationToken)
        {
            const string sql = @"IF NOT EXISTS (SELECT 1 FROM dbo.Posts WHERE Id = @Id)
                                BEGIN
                                    INSERT INTO dbo.Posts  (Id, UserId, Title, Body, FetchedAtUtc)
                                    VALUES (@Id, @UserId, @Title, @Body, @FetchedAtUtc)
                                END";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            AddParameters(command, post);
            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task InsertManyAsync(IEnumerable<PostRecord> posts, CancellationToken cancellationToken)
        {
            const string sql = @"IF NOT EXISTS (SELECT 1 FROM dbo.Posts WHERE Id = @Id)
                                 BEGIN
                                    INSERT INTO dbo.Posts  (Id, UserId, Title, Body, FetchedAtUtc)
                                    VALUES (@Id, @UserId, @Title, @Body, @FetchedAtUtc)
                                 END";
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            var sqlTransaction = (SqlTransaction)transaction;

            try
            {
                foreach (var post in posts)
                {
                    await using var command = new SqlCommand(sql, connection, sqlTransaction);
                    AddParameters(command, post);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        
    }
}

