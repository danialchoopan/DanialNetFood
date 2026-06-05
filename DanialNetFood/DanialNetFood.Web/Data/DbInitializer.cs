using DanialNetFood.Web.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace DanialNetFood.Web.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any()) return;

            // Seed Users & Wallets
            var users = new List<User>();
            var roles = new[] { "Customer", "RestaurantOwner", "Driver", "SuperAdmin" };

            // Core users
            users.Add(new User { Username = "customer", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = "Customer", Latitude = 35.70, Longitude = 51.40 });
            users.Add(new User { Username = "owner", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = "RestaurantOwner" });
            users.Add(new User { Username = "driver", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = "Driver" });
            users.Add(new User { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = "SuperAdmin" });

            // Extra users for scale
            for(int i = 1; i <= 20; i++) {
                users.Add(new User { Username = $"owner{i}", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = "RestaurantOwner" });
                users.Add(new User { Username = $"driver{i}", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = "Driver" });
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            foreach(var u in context.Users) {
                context.Wallets.Add(new Wallet { UserId = u.Id, Balance = 1000000 });
            }
            context.SaveChanges();

            // Seed Restaurants (20+)
            var restaurants = new List<Restaurant>();
            for(int i = 1; i <= 20; i++) {
                restaurants.Add(new Restaurant {
                    Name = $"رستوران شماره {i}",
                    Description = $"توضیحات رستوران {i} با بهترین کیفیت",
                    ImageUrl = $"/images/restaurant{(i%3)+1}.jpg",
                    OwnerId = context.Users.First(u => u.Username == $"owner{i}").Id,
                    Latitude = 35.70 + (i * 0.001),
                    Longitude = 51.40 + (i * 0.001),
                    ServiceRadiusKm = 5.0
                });
            }
            context.Restaurants.AddRange(restaurants);
            context.SaveChanges();

            // Seed Foods & Options
            foreach(var r in restaurants) {
                var foods = new List<Food> {
                    new Food { Name = "چلو کباب سلطانی", Price = 350000, Category = "ایرانی", RestaurantId = r.Id, StockQuantity = 100 },
                    new Food { Name = "پیتزا مخصوص", Price = 280000, Category = "فست‌فود", RestaurantId = r.Id, StockQuantity = 50 }
                };
                context.Foods.AddRange(foods);
                context.SaveChanges();

                foreach(var f in foods) {
                    context.FoodOptions.AddRange(new List<FoodOption> {
                        new FoodOption { Name = "نوشابه", Price = 25000, FoodId = f.Id, StockQuantity = 200 },
                        new FoodOption { Name = "سس اضافه", Price = 5000, FoodId = f.Id, StockQuantity = 500 }
                    });
                }
            }
            context.SaveChanges();

            var discounts = new List<DiscountCode>
            {
                new DiscountCode { Code = "WELCOME", Type = "Percentage", Value = 20, IsActive = true },
                new DiscountCode { Code = "FIXED50", Type = "Fixed", Value = 50000, IsActive = true }
            };
            context.DiscountCodes.AddRange(discounts);
            context.SaveChanges();
        }
    }
}
