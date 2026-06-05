using Microsoft.EntityFrameworkCore;
using DanialNetFood.Web.Models;

namespace DanialNetFood.Web.Data.Repositories
{
    public interface IRestaurantRepository : IRepository<Restaurant>
    {
        Task<Restaurant?> GetRestaurantWithMenuAsync(int id);
    }

    public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
    {
        public RestaurantRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Restaurant?> GetRestaurantWithMenuAsync(int id)
        {
            return await _context.Restaurants.Include(r => r.Menu).FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
