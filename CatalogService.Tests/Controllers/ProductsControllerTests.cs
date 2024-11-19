using CatalogService.Controllers;
using CatalogService.Data;
using CatalogService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CatalogService.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly DbContextOptions<CatalogContext> _dbContextOptions;

        public ProductsControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<CatalogContext>()
                .UseInMemoryDatabase(databaseName: "CatalogTestDb")
                .Options;
        }

        [Fact]
        public async Task GetProducts_ReturnsAllProducts()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Products.Add(new Product { Id = 1, Name = "Product A", Price = 10.99M });
            context.Products.Add(new Product { Id = 2, Name = "Product B", Price = 20.50M });
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProducts();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
            Assert.Equal(2, products.Count());
        }

        [Fact]
        public async Task GetProduct_ReturnsProductById()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Products.Add(new Product { Name = "Product A", Price = 10.99M });
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProduct(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var product = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal("Product A", product.Name);
        }

        [Fact]
        public async Task PostProduct_CreatesNewProduct_WithAutoIncrementedId()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            var controller = new ProductsController(context);
            var newProduct = new Product
            {
                Name = "Product A",
                Description = "Test Product",
                CostPrice = 100,
                ProfitMargin = 60, // Above 55%
                SalePrice = 180,  // Greater than Price
                PromotionalPrice = 160, // Greater than Price
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var createdProduct = Assert.IsType<Product>(actionResult.Value);

            Assert.Equal(1, createdProduct.Id); // First auto-incremented ID
            Assert.Equal(160, createdProduct.Price); // CostPrice + 60% ProfitMargin
            Assert.Equal(1, context.Products.Count());
        }

        [Fact]
        public async Task PutProduct_UpdatesExistingProduct()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Products.Add(new Product { Id = 1, Name = "Product A", Price = 10.99M });
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);
            var updatedProduct = new Product { Id = 1, Name = "Updated Product", Price = 12.99M };

            // Act
            var result = await controller.PutProduct(1, updatedProduct);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var product = await context.Products.FindAsync(1);
            Assert.Equal("Updated Product", product.Name);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProduct()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Products.Add(new Product { Id = 1, Name = "Product A", Price = 10.99M });
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);

            // Act
            var result = await controller.DeleteProduct(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(0, context.Products.Count());
        }

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProduct(99); // ID inexistente

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task PutProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            var controller = new ProductsController(context);
            var updatedProduct = new Product { Id = 99, Name = "Nonexistent Product", Price = 15.99M }; // ID inexistente

            // Act
            var result = await controller.PutProduct(99, updatedProduct);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PutProduct_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Products.Add(new Product { Id = 1, Name = "Product A", Price = 10.99M });
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);
            var updatedProduct = new Product { Id = 2, Name = "Updated Product", Price = 12.99M }; // ID diferente

            // Act
            var result = await controller.PutProduct(1, updatedProduct);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            var controller = new ProductsController(context);

            // Act
            var result = await controller.DeleteProduct(99); // ID inexistente

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostProduct_CreatesNewProduct_WithCalculatedPrice()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            var controller = new ProductsController(context);
            var newProduct = new Product
            {
                Name = "Product A",
                Description = "Test Product",
                CostPrice = 100,
                ProfitMargin = 60, // Above 55%
                SalePrice = 180,  // Greater than Price
                PromotionalPrice = 160, // Greater than Price
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var createdProduct = Assert.IsType<Product>(actionResult.Value);

            Assert.Equal(160, createdProduct.Price); // CostPrice + 60% ProfitMargin
            Assert.Equal(1, context.Products.Count());
        }

        [Fact]
        public async Task PostProduct_ReturnsBadRequest_WhenProfitMarginIsBelow55()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            var controller = new ProductsController(context);
            var newProduct = new Product
            {
                Name = "Product A",
                Description = "Test Product",
                CostPrice = 100,
                ProfitMargin = 50, // Below 55%
                SalePrice = 180,
                PromotionalPrice = 160,
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PostProduct_ReturnsBadRequest_WhenSalePriceOrPromotionalPriceIsInvalid()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            var controller = new ProductsController(context);
            var newProduct = new Product
            {
                Name = "Product A",
                Description = "Test Product",
                CostPrice = 100,
                ProfitMargin = 60, // Above 55%
                SalePrice = 150,  // Less than Price
                PromotionalPrice = 140, // Less than Price
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}