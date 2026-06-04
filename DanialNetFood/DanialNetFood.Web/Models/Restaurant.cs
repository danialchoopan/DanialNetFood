using System.ComponentModel.DataAnnotations;

namespace DanialNetFood.Web.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<Food> Menu { get; set; } = new();
    }
}
