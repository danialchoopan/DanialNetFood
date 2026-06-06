using Microsoft.EntityFrameworkCore;
using DanialNetFood.Web.Models;

namespace DanialNetFood.Web.Data.Repositories
{
    public interface IRestaurantRepository : IRepository<Restaurant>
    {
        Task<Restaurant?> GetRestaurantWithMenuAsync(int id);
        Task<Restaurant?> GetRestaurantByOwnerIdAsync(int ownerId);
    }

    public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
    {
        public RestaurantRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Restaurant?> GetRestaurantWithMenuAsync(int id)
        {
            return await _context.Restaurants.Include(r => r.Menu).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Restaurant?> GetRestaurantByOwnerIdAsync(int ownerId)
        {
            return await _context.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == ownerId);
        }
    }
}
