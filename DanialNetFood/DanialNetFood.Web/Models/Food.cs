using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanialNetFood.Web.Models
{
    public class Food
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
        [ForeignKey("RestaurantId")]
        public Restaurant Restaurant { get; set; } = null!;

        public int StockQuantity { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public List<FoodOption> Options { get; set; } = new();
    }
}
