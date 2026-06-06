using System.ComponentModel.DataAnnotations;

namespace DanialNetFood.Web.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = "Customer"; // Customer, RestaurantOwner, Driver, SuperAdmin

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public Wallet? Wallet { get; set; }
    }
}
