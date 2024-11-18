using System.ComponentModel.DataAnnotations;

namespace CatalogService.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "CostPrice must be greater than 0.")]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(55, 100, ErrorMessage = "ProfitMargin must be at least 55%.")]
        public decimal ProfitMargin { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "SalePrice must be greater than 0.")]
        public decimal SalePrice { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "PromotionalPrice must be greater than 0.")]
        public decimal PromotionalPrice { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a positive value.")]
        public int Stock { get; set; }

        /// <summary>
        /// Calculate Price based on CostPrice and ProfitMargin
        /// </summary>
        public void CalculatePrice()
        {
            Price = CostPrice + (CostPrice * ProfitMargin / 100);
        }

        /// <summary>
        /// Validate SalePrice and PromotionalPrice
        /// </summary>
        public bool ValidatePrices()
        {
            return SalePrice > Price && PromotionalPrice > Price;
        }
    }
}