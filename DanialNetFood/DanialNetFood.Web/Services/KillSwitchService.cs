using Microsoft.Extensions.Caching.Memory;

namespace DanialNetFood.Web.Services
{
    public interface IKillSwitchService
    {
        bool IsSystemActive();
        void SetSystemStatus(bool isActive);
        bool IsRestaurantActive(int restaurantId);
        void SetRestaurantStatus(int restaurantId, bool isActive);
    }

    public class KillSwitchService : IKillSwitchService
    {
        private readonly IMemoryCache _cache;
        private const string GlobalKey = "KillSwitch_Global";
        private const string RestaurantKeyPrefix = "KillSwitch_Restaurant_";

        public KillSwitchService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool IsSystemActive()
        {
            return _cache.Get<bool?>(GlobalKey) ?? true;
        }

        public void SetSystemStatus(bool isActive)
        {
            _cache.Set(GlobalKey, isActive);
        }

        public bool IsRestaurantActive(int restaurantId)
        {
            return _cache.Get<bool?>(RestaurantKeyPrefix + restaurantId) ?? true;
        }

        public void SetRestaurantStatus(int restaurantId, bool isActive)
        {
            _cache.Set(RestaurantKeyPrefix + restaurantId, isActive);
        }
    }
}
