using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DanialNetFood.Web.Data.UnitOfWork;

namespace DanialNetFood.Web.Controllers
{
    [Authorize(Roles = "RestaurantOwner")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return View(orders);
        }

        public async Task<IActionResult> ManageMenu()
        {
            var foods = await _unitOfWork.Foods.GetAllAsync();
            return View(foods);
        }
    }
}
