using DanialNetFood.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace DanialNetFood.Web.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (context.Restaurants.Any()) return;

            var hasher = new PasswordHasher<User>();

            var restaurants = new List<Restaurant>
            {
                new Restaurant {
                    Name = "رستوران دانیال",
                    Description = "بهترین غذاهای سنتی ایرانی",
                    ImageUrl = "/images/rest1.jpg",
                    Menu = new List<Food>
                    {
                        new Food { Name = "چلو کباب سلطانی", Price = 350000, Category = "ایرانی" },
                        new Food { Name = "خورشت قورمه سبزی", Price = 180000, Category = "ایرانی" }
                    }
                },
                new Restaurant {
                    Name = "پیتزا شب",
                    Description = "پیتزاهای تنوری و برگرهای ذغالی",
                    ImageUrl = "/images/rest2.jpg",
                    Menu = new List<Food>
                    {
                        new Food { Name = "پیتزا مخلوط مخصوص", Price = 220000, Category = "فست فود" },
                        new Food { Name = "چیزبرگر دوبل", Price = 195000, Category = "فست فود" }
                    }
                }
            };

            context.Restaurants.AddRange(restaurants);

            var admin = new User { Username = "admin", Role = "RestaurantOwner" };
            admin.PasswordHash = hasher.HashPassword(admin, "admin123");

            var user = new User { Username = "user", Role = "Customer" };
            user.PasswordHash = hasher.HashPassword(user, "user123");

            context.Users.AddRange(admin, user);

            var discountCodes = new List<DiscountCode>
            {
                new DiscountCode { Code = "OFF20", Type = "Percentage", Value = 20 },
                new DiscountCode { Code = "FIX50", Type = "FixedAmount", Value = 50000 }
            };

            context.DiscountCodes.AddRange(discountCodes);

            context.SaveChanges();
        }
    }
}
