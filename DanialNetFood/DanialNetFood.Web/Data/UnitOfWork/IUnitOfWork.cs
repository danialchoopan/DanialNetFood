using DanialNetFood.Web.Models;
using DanialNetFood.Web.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DanialNetFood.Web.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRestaurantRepository Restaurants { get; }
        IRepository<Food> Foods { get; }
        IRepository<FoodOption> FoodOptions { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderItems { get; }
        IRepository<OrderItemOption> OrderItemOptions { get; }
        IRepository<DiscountCode> DiscountCodes { get; }
        IRepository<Wallet> Wallets { get; }
        IRepository<WalletTransaction> WalletTransactions { get; }
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new Repository<User>(_context);
            Restaurants = new RestaurantRepository(_context);
            Foods = new Repository<Food>(_context);
            FoodOptions = new Repository<FoodOption>(_context);
            Orders = new Repository<Order>(_context);
            OrderItems = new Repository<OrderItem>(_context);
            OrderItemOptions = new Repository<OrderItemOption>(_context);
            DiscountCodes = new Repository<DiscountCode>(_context);
            Wallets = new Repository<Wallet>(_context);
            WalletTransactions = new Repository<WalletTransaction>(_context);
        }

        public IRepository<User> Users { get; private set; }
        public IRestaurantRepository Restaurants { get; private set; }
        public IRepository<Food> Foods { get; private set; }
        public IRepository<FoodOption> FoodOptions { get; private set; }
        public IRepository<Order> Orders { get; private set; }
        public IRepository<OrderItem> OrderItems { get; private set; }
        public IRepository<OrderItemOption> OrderItemOptions { get; private set; }
        public IRepository<DiscountCode> DiscountCodes { get; private set; }
        public IRepository<Wallet> Wallets { get; private set; }
        public IRepository<WalletTransaction> WalletTransactions { get; private set; }

        public async Task<int> CompleteAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // In a real scenario, we might reload and retry or throw a custom domain exception
                throw new Exception("تداخل در به‌روزرسانی داده‌ها. لطفاً دوباره تلاش کنید.");
            }
        }

        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync() => await _context.Database.CommitTransactionAsync();

        public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

        public void Dispose() => _context.Dispose();
    }
}
