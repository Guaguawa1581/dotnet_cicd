using ApiServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepos _productRepos;

        public ProductsController(IProductRepos ProductRepos)
        {
            _productRepos = ProductRepos;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> Get()
        {
            var products = await _productRepos.GetAllAsync();
            return Ok(products);
        }

        /// <summary>
        /// 非同步大量新增產品（使用 SqlBulkCopy）
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkInsert(
            [FromBody] IEnumerable<Product> products,
            CancellationToken cancellationToken)
        {
            var list = products?.ToList();
            if (list == null || list.Count == 0)
                return BadRequest("產品清單不可為空。");

            await _productRepos.BulkInsertAsync(list, cancellationToken);
            return Ok(new { inserted = list.Count });
        }
    }
}
