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
        public int OwnerId { get; set; }

        // Geofencing
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double ServiceRadiusKm { get; set; } = 5.0;

        public List<Food> Menu { get; set; } = new();
    }
}
