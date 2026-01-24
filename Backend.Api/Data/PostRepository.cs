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
    }
}

