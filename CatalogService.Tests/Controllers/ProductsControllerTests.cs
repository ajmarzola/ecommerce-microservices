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

            // Aqui capturamos o `CreatedAtActionResult` e extraímos o valor do produto
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdProduct = Assert.IsType<Product>(createdAtActionResult.Value);

            // Validar os dados do produto criado
            Assert.NotNull(createdProduct);
            Assert.NotEqual(0, createdProduct.Id); // Verifica se o ID foi gerado
            Assert.Equal("Product A", createdProduct.Name);
            Assert.Equal(160, createdProduct.Price); // CostPrice + ProfitMargin
            Assert.Equal(1, context.Products.Count()); // Confirma que o produto foi adicionado ao banco
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
                CostPrice = 100, // Preço de custo válido
                ProfitMargin = 60, // Margem de lucro >= 55%
                SalePrice = 180,  // Maior que Price (calculado)
                PromotionalPrice = 165, // Maior que Price
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);

            // Capturar o resultado do CreatedAtAction
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdProduct = Assert.IsType<Product>(createdAtActionResult.Value);

            // Validar que o ID foi gerado automaticamente
            Assert.NotNull(createdProduct);
            Assert.True(createdProduct.Id > 0, "The Id should be greater than 0 for an auto-incremented field.");
            Assert.Equal("Product A", createdProduct.Name);
            Assert.Equal(160, createdProduct.Price); // Calculado com base no CostPrice e ProfitMargin
            Assert.Equal(1, context.Products.Count()); // Banco contém um produto
        }


        [Fact]
        public async Task PutProduct_UpdatesExistingProduct()
        {
            // Arrange
            using var context = new CatalogContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new ProductsController(context);
            var newProduct = new Product
            {
                Name = "Product A",
                Description = "Test Product",
                CostPrice = 100, // Preço de custo válido
                ProfitMargin = 60, // Margem de lucro >= 55%
                SalePrice = 180,  // Maior que Price (calculado)
                PromotionalPrice = 165, // Maior que Price
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);

            // Capturar o resultado do CreatedAtAction
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdProduct = Assert.IsType<Product>(createdAtActionResult.Value);

            // Validar os dados do produto criado
            Assert.NotNull(createdProduct);
            Assert.NotEqual(0, createdProduct.Id); // ID autoincrementado
            Assert.Equal("Product A", createdProduct.Name);
            Assert.Equal(160, createdProduct.Price); // Calculado com base no CostPrice e ProfitMargin
            Assert.Equal(1, context.Products.Count()); // Banco contém um produto
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
                ProfitMargin = 50, // Margem de lucro abaixo de 55%
                SalePrice = 180,
                PromotionalPrice = 165,
                Category = "Category A",
                Stock = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);

            // Verificar que o resultado é um BadRequestObjectResult
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            // Validar a mensagem de erro retornada
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
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}