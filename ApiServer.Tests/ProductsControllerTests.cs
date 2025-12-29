using System.Collections.Generic;
using ApiServer.Controllers;
using ApiServer.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ApiServer.Tests
{
    public class ProductsControllerTests
    {
        [Fact]
        public void Get_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange
            var controller = new ProductsController();

            // Act
            var result = controller.Get();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Product>>(actionResult.Value);
            Assert.True(returnValue.Count >= 3);
        }

        [Fact]
        public void Get_ById_ReturnsOkResult()
        {
            // Arrange
            var controller = new ProductsController();

            // Act
            var result = controller.Get(1);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var product = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal(1, product.Id);
        }

        [Fact]
        public void Post_CreatesProduct()
        {
            // Arrange
            var controller = new ProductsController();
            var newProduct = new Product { Name = "NewOne", Price = 12.34m };

            // Act
            var result = controller.Post(newProduct);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var product = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal("NewOne", product.Name);
            Assert.NotEqual(0, product.Id);
        }
    }
}
