using Microsoft.EntityFrameworkCore;
using DanialNetFood.Web.Models;

namespace DanialNetFood.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<FoodOption> FoodOptions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemOption> OrderItemOptions { get; set; }
        public DbSet<DiscountCode> DiscountCodes { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Food>()
                .Property(f => f.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<FoodOption>()
                .Property(fo => fo.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Wallet>()
                .HasKey(w => w.UserId);
        }
    }
}
