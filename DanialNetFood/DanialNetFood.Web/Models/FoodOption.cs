using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanialNetFood.Web.Models
{
    public class FoodOption
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int FoodId { get; set; }
        [ForeignKey("FoodId")]
        public Food Food { get; set; } = null!;

        public int StockQuantity { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
