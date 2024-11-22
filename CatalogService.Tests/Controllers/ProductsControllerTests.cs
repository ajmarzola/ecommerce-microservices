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
            context.Database.EnsureDeleted(); // Reset database
            context.Database.EnsureCreated(); // Recreate database schema

            context.Products.Add(new Product { Name = "Product A", CostPrice = 100, ProfitMargin = 60, SalePrice = 180, PromotionalPrice = 160, Category = "Category A", Stock = 10 });
            context.Products.Add(new Product { Name = "Product B", CostPrice = 200, ProfitMargin = 70, SalePrice = 400, PromotionalPrice = 350, Category = "Category B", Stock = 5 });
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
        public async Task PostProduct_CreatesNewProduct()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new ProductsController(context);
            var newProduct = new Product
            {
                Name = "Product A",
                Description = "Test Product",
                CostPrice = 100,
                ProfitMargin = 60,
                SalePrice = 180,
                PromotionalPrice = 165,
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);

            // Here we capture the `CreatedAtActionResult` and extract the product value
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdProduct = Assert.IsType<Product>(createdAtActionResult.Value);

            // Validate the data of the created product
            Assert.NotNull(createdProduct);
            Assert.NotEqual(0, createdProduct.Id); // Check if the ID was generated
            Assert.Equal("Product A", createdProduct.Name);
            Assert.Equal(160, createdProduct.Price); // Cost Price + Profit Margin
            Assert.Equal(1, context.Products.Count()); // Confirms that the product has been added to the database
        }

        [Fact]
        public async Task PostProduct_CreatesNewProduct_WithAutoIncrementedId()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new ProductsController(context);
            var newProduct = new Product
            {
                Name = "Product A",
                Description = "Test Product",
                CostPrice = 100, // Valid cost price
                ProfitMargin = 60, // Profit margin >= 55%
                SalePrice = 180,  // Greater than Price (calculated)
                PromotionalPrice = 165, // Greater than Price
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);

            // Capture the result of CreatedAtAction
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdProduct = Assert.IsType<Product>(createdAtActionResult.Value);

            // Validate that the ID was automatically generated
            Assert.NotNull(createdProduct);
            Assert.True(createdProduct.Id > 0, "The Id should be greater than 0 for an auto-incremented field.");
            Assert.Equal("Product A", createdProduct.Name);
            Assert.Equal(160, createdProduct.Price); // Calculated based on CostPrice and ProfitMargin
            Assert.Equal(1, context.Products.Count()); // Database contains a product
        }


        [Fact]
        public async Task PutProduct_UpdatesExistingProduct()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var product = new Product
            {
                Id = 1,
                Name = "Existing Product",
                Description = "Original Description",
                CostPrice = 100,
                ProfitMargin = 60,
                SalePrice = 160,
                Stock = 10,
                Category = "Category A"
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);

            product.Name = "Updated Product";
            product.CostPrice = 80; // Valid cost price
            product.SalePrice = 200;  // Greater than Price (calculated)
            product.PromotionalPrice = 180; // Greater than Price
            product.Stock = 15;

            // Act
            var result = await controller.PutProduct(product.Id, product);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var productInDb = await context.Products.FindAsync(product.Id);
            Assert.NotNull(productInDb);
            Assert.Equal("Updated Product", productInDb.Name);
            Assert.Equal(80, productInDb.CostPrice);
            Assert.Equal(200, productInDb.SalePrice);
            Assert.Equal(15, productInDb.Stock);
            Assert.Equal(180, productInDb.PromotionalPrice);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProduct()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Product A",
                CostPrice = 100, // Valid cost price
                ProfitMargin = 60, // Profit margin >= 55%
                SalePrice = 180,  // Greater than Price (calculated)
                PromotionalPrice = 165, // Greater than Price
                Category = "Category A",
                Stock = 10
            });

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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProduct(99); // Non-existent ID

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task PutProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new ProductsController(context);

            var updatedProduct = new Product
            {
                Id = 99, // Non-existent ID
                Name = "Nonexistent Product",
                Description = "Test Product",
                CostPrice = 100,
                ProfitMargin = 60,
                SalePrice = 180,
                PromotionalPrice = 165,
                Category = "Category A",
                Stock = 10
            };

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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Product A",
                CostPrice = 100, // Valid cost price
                ProfitMargin = 60, // Profit margin >= 55%
                SalePrice = 180,  // Greater than Price (calculated)
                PromotionalPrice = 165, // Greater than Price
                Category = "Category A",
                Stock = 10
            });

            await context.SaveChangesAsync();

            var controller = new ProductsController(context);
            var updatedProduct = new Product { Id = 2, Name = "Updated Product" }; // ID diferente

            // Act
            var result = await controller.PutProduct(1, updatedProduct);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new ProductsController(context);

            // Act
            var result = await controller.DeleteProduct(99); // Non-existent ID

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostProduct_CreatesNewProduct_WithCalculatedPrice()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new ProductsController(context);
            var newProduct = new Product
            {
                Name = "Product A",
                Description = "Test Product",
                CostPrice = 100, // Valid cost price
                ProfitMargin = 60, // Profit margin >= 55%
                SalePrice = 180,  // Greater than Price (calculated)
                PromotionalPrice = 165, // Greater than Price
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);

            // Capture the result of CreatedAtAction
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdProduct = Assert.IsType<Product>(createdAtActionResult.Value);

            // Validate the data of the created product
            Assert.NotNull(createdProduct);
            Assert.NotEqual(0, createdProduct.Id); // Auto-incremented ID
            Assert.Equal("Product A", createdProduct.Name);
            Assert.Equal(160, createdProduct.Price); // Calculated based on CostPrice and ProfitMargin
            Assert.Equal(1, context.Products.Count()); // Database contains a product
        }

        [Fact]
        public async Task PostProduct_ReturnsBadRequest_WhenProfitMarginIsBelow55()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new ProductsController(context);
            var newProduct = new Product
            {
                Name = "Product A",
                Description = "Test Product",
                CostPrice = 100,
                ProfitMargin = 50, // Profit margin below 55%
                SalePrice = 180,
                PromotionalPrice = 165,
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);

            // Check that the result is a BadRequestObjectResult
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            // Validate the returned error message
            Assert.Equal("ProfitMargin must be at least 55%.", badRequestResult.Value);
        }


        [Fact]
        public async Task PostProduct_ReturnsBadRequest_WhenSalePriceOrPromotionalPriceIsInvalid()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}