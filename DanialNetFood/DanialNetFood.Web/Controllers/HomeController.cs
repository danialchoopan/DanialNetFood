using DanialNetFood.Web.Data.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace DanialNetFood.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var restaurants = await _unitOfWork.Restaurants.GetAllAsync();
            return View(restaurants);
        }

        public async Task<IActionResult> Menu(int id)
        {
            var restaurant = await _unitOfWork.Restaurants.GetRestaurantWithMenuAsync(id);
            if (restaurant == null) return NotFound();
            return View(restaurant);
        }
    }
}
