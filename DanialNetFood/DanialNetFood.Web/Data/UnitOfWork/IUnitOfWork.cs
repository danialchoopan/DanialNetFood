using DanialNetFood.Web.Models;
using DanialNetFood.Web.Data.Repositories;

namespace DanialNetFood.Web.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRestaurantRepository Restaurants { get; }
        IRepository<Food> Foods { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderItems { get; }
        IRepository<DiscountCode> DiscountCodes { get; }
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
            Orders = new Repository<Order>(_context);
            OrderItems = new Repository<OrderItem>(_context);
            DiscountCodes = new Repository<DiscountCode>(_context);
        }

        public IRepository<User> Users { get; private set; }
        public IRestaurantRepository Restaurants { get; private set; }
        public IRepository<Food> Foods { get; private set; }
        public IRepository<Order> Orders { get; private set; }
        public IRepository<OrderItem> OrderItems { get; private set; }
        public IRepository<DiscountCode> DiscountCodes { get; private set; }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync() => await _context.Database.CommitTransactionAsync();

        public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

        public void Dispose() => _context.Dispose();
    }
}
