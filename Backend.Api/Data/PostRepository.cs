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
            var raw = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("❗ConnectionString not set");
            _connectionString = NormalizeConnectionString(raw);
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
            const string sql = @"SELECT Id, UserId, Title, Body, FetchedAtUtc FROM dbo.Posts ORDER BY Id";
            
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

        private static void AddParameters(SqlCommand command, PostRecord post)
        {
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = post.Id });
            command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = post.UserId });
            command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = post.Title });
            command.Parameters.Add(new SqlParameter("@Body", SqlDbType.NVarChar, -1) { Value = post.Body });
            command.Parameters.Add(new SqlParameter("@FetchedAtUtc", SqlDbType.DateTime2) { Value = post.FetchedAtUtc });
        }

        private static PostRecord Map(SqlDataReader reader)
        {
            return new PostRecord
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Title = reader.GetString(2),
                Body = reader.GetString(3),
                FetchedAtUtc = reader.GetDateTime(4),
            };
        }

        private static string NormalizeConnectionString(string connectionString)
        {
            var segments = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var normalized = new List<string>(segments.Length);

            foreach (var segment in segments)
            {
                var trimmed = segment.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                var equalsIndex = trimmed.IndexOf('=');
                if (equalsIndex <= 0)
                {
                    normalized.Add(trimmed);
                    continue;
                }

                var key = trimmed[..equalsIndex];
                var value = trimmed[(equalsIndex + 1)..];

                var normalizedKey = string.Join(" ", key.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
                var normalizedValue = value.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
                normalized.Add($"{normalizedKey}={normalizedValue}");
            }

            return string.Join(";", normalized) + ";";
        }
    }
}
