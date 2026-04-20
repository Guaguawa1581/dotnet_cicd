using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ApiServer.Infrastructure;

namespace ApiServer.Models
{
    public class ProductModel
    {
        public string? PdLogID { get; set; }
        public string PdName { get; set; } = string.Empty;
        public int PdPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public interface IProductRepos
    {
        Task<IEnumerable<ProductModel>> GetAllAsync();
        Task BulkInsertAsync(IEnumerable<ProductModel> products, CancellationToken cancellationToken = default);
    }

    public class ProductRepos(IDbConnectionFactory db) : IProductRepos
    {
        private readonly IDbConnectionFactory _db = db;

        public async Task<IEnumerable<ProductModel>> GetAllAsync()
        {
            await using var connection = (SqlConnection)await _db.OpenAsync();
            return await connection.QueryAsync<ProductModel>(
                "SELECT pdLogID, pdName, pdPrice, CreatedAt FROM guaProduct");
        }

        public async Task BulkInsertAsync(IEnumerable<ProductModel> products, CancellationToken cancellationToken = default)
        {
            var list = products.ToList();
            var logIds = await _db.GenerateLogIdsAsync(list.Count, cancellationToken);

            var table = new DataTable();
            table.Columns.Add("pdLogID", typeof(string));
            table.Columns.Add("pdName", typeof(string));
            table.Columns.Add("pdPrice", typeof(int));

            for (int i = 0; i < list.Count; i++)
                table.Rows.Add(logIds[i], list[i].PdName, list[i].PdPrice);

            await using var connection = (SqlConnection)await _db.OpenAsync(cancellationToken);

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "guaProduct",
                BatchSize = 1000
            };

            bulkCopy.ColumnMappings.Add("pdLogID", "pdLogID");
            bulkCopy.ColumnMappings.Add("pdName", "pdName");
            bulkCopy.ColumnMappings.Add("pdPrice", "pdPrice");

            await bulkCopy.WriteToServerAsync(table, cancellationToken);
        }
    }
}
