using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiServer.Infrastructure
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> OpenAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>> GenerateLogIdsAsync(int count, CancellationToken cancellationToken = default);
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ILogger<DbConnectionFactory> _logger;

        public DbConnectionFactory(IConfiguration configuration, ILogger<DbConnectionFactory> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            _logger = logger;
        }

        public async Task<IDbConnection> OpenAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync(cancellationToken);
                _logger.LogInformation("DB connection opened. DataSource={DataSource}", connection.DataSource);
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open DB connection. DataSource={DataSource}", connection.DataSource);
                await connection.DisposeAsync();
                throw;
            }
        }

        // 用系統內建 sp_sequence_get_range 一次保留 count 個 sequence 值，不需要自定預存程序
        public async Task<IReadOnlyList<string>> GenerateLogIdsAsync(int count, CancellationToken cancellationToken = default)
        {
            await using var connection = (SqlConnection)await OpenAsync(cancellationToken);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "sys.sp_sequence_get_range";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@sequence_name", "dbo.seq_getLogID");
            cmd.Parameters.AddWithValue("@range_size", count);
            var firstValueParam = cmd.Parameters.Add("@range_first_value", SqlDbType.Variant);
            firstValueParam.Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            var firstSeq = Convert.ToInt64(firstValueParam.Value);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            return [.. Enumerable.Range(0, count)
                .Select(i => timestamp + ((firstSeq + i - 1) % 9_999_999 + 1).ToString("D7"))];
        }
    }
}
