using DanialNetFood.Web.Data.UnitOfWork;
using DanialNetFood.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DanialNetFood.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private const string RestaurantsCacheKey = "AllRestaurants";
        private const string RestaurantMenuCacheKeyPrefix = "RestaurantMenu_";

        public HomeController(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            if (!_cache.TryGetValue(RestaurantsCacheKey, out IEnumerable<Restaurant>? restaurants))
            {
                restaurants = await _unitOfWork.Restaurants.GetAllAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                _cache.Set(RestaurantsCacheKey, restaurants, cacheOptions);
            }
            return View(restaurants);
        }

        public async Task<IActionResult> Menu(int id)
        {
            var cacheKey = RestaurantMenuCacheKeyPrefix + id;
            if (!_cache.TryGetValue(cacheKey, out Restaurant? restaurant))
            {
                restaurant = await _unitOfWork.Restaurants.GetRestaurantWithMenuAsync(id);
                if (restaurant != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                    _cache.Set(cacheKey, restaurant, cacheOptions);
                }
            }

            if (restaurant == null) return NotFound();
            return View(restaurant);
        }
    }
}
