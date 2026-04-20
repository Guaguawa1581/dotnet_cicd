using System.Data;
using Microsoft.Data.SqlClient;
using ApiServer.Infrastructure;

namespace ApiServer.Models
{
    public class LogDto
    {
        public string LogId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }
    }

    public class DuplicateLogIdException : Exception
    {
        public DuplicateLogIdException()
            : base("LogId 重複，請重新送出。") { }
    }

    public interface ILogRepos
    {
        Task BulkInsertAsync(IEnumerable<LogDto> logs, CancellationToken cancellationToken = default);
    }

    public class LogRepos(IDbConnectionFactory db) : ILogRepos
    {
        private readonly IDbConnectionFactory _db = db;

        public async Task BulkInsertAsync(IEnumerable<LogDto> logs, CancellationToken cancellationToken = default)
        {
            var list = logs.ToList();
            var table = BuildDataTable(list);

            await using var connection = (SqlConnection)await _db.OpenAsync(cancellationToken);
            var transaction = connection.BeginTransaction();
            try
            {
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
                {
                    DestinationTableName = "Logs",
                    BatchSize = 1000
                };
                bulkCopy.ColumnMappings.Add("LogId",    "LogId");
                bulkCopy.ColumnMappings.Add("Message",  "Message");
                bulkCopy.ColumnMappings.Add("CreateAt", "CreateAt");

                await bulkCopy.WriteToServerAsync(table, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new DuplicateLogIdException();
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static DataTable BuildDataTable(IEnumerable<LogDto> logs)
        {
            var table = new DataTable();
            table.Columns.Add("LogId",    typeof(string));
            table.Columns.Add("Message",  typeof(string));
            table.Columns.Add("CreateAt", typeof(DateTime));

            foreach (var log in logs)
                table.Rows.Add(log.LogId, log.Message, log.CreateAt);

            return table;
        }
    }
}
