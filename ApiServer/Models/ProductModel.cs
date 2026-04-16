using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiServer.Models
{
    public interface IProductRepos
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task BulkInsertAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);
    }
    public class ProductRepos : IProductRepos
    {
        private readonly string _connectionString;

        public ProductRepos(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Product>(
                "SELECT Id, Name, Price FROM Products");
        }

        public async Task BulkInsertAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
        {
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Price", typeof(decimal));

            foreach (var product in products)
            {
                table.Rows.Add(product.Name, product.Price);
            }

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "Products",
                BatchSize = 1000
            };

            bulkCopy.ColumnMappings.Add("Name", "Name");
            bulkCopy.ColumnMappings.Add("Price", "Price");

            await bulkCopy.WriteToServerAsync(table, cancellationToken);
        }
    }
}
