using CatalogService.Data;
using CatalogService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers
{
    /// <summary>
    /// Controller to manage products in the system.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly CatalogContext _context;

        public ProductsController(CatalogContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all products.
        /// </summary>
        /// <returns>A list of products.</returns>
        /// <response code="200">Returns the list of products.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <returns>The product corresponding to the ID.</returns>
        /// <response code="200">Returns the product.</response>
        /// <response code="404">Product not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return product;
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">The object containing the product data to be created.</param>
        /// <returns>The newly created product.</returns>
        /// <response code="201">Product successfully created.</response>
        /// <response code="400">Bad request.</response>
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            product.CalculatePrice();

            if (!product.ValidatePrices())
            {
                return BadRequest("SalePrice and PromotionalPrice must be greater than Price.");
            }

            if (!product.ValidadeProfitMargin())
            {
                return BadRequest("ProfitMargin must be at least 55%.");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Make sure to return the created product correctly
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <param name="product">The object containing the new product data.</param>
        /// <returns>No content if the update is successful.</returns>
        /// <response code="204">Product successfully updated.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="404">Product not found.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            var existingProduct = _context.Products.Find(id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            product.CalculatePrice();

            if (!product.ValidatePrices())
            {
                return BadRequest("SalePrice and PromotionalPrice must be greater than Price.");
            }

            if (!product.ValidadeProfitMargin())
            {
                return BadRequest("ProfitMargin must be at least 55%.");
            }

            // Update existing product values
            _context.Entry(existingProduct).CurrentValues.SetValues(product);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to be deleted.</param>
        /// <response code="204">Product successfully deleted.</response>
        /// <response code="404">Product not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}