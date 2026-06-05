using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DanialNetFood.Web.Data.UnitOfWork;
using DanialNetFood.Web.Models;
using Microsoft.AspNetCore.SignalR;
using DanialNetFood.Web.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DanialNetFood.Web.Controllers
{
    [Authorize(Roles = "RestaurantOwner")]
    public class RestaurantAdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<OrderHub> _orderHub;
        private readonly IMemoryCache _cache;

        public RestaurantAdminController(IUnitOfWork unitOfWork, IHubContext<OrderHub> orderHub, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _orderHub = orderHub;
            _cache = cache;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        public async Task<IActionResult> LiveOrders()
        {
            var restaurant = await _unitOfWork.Restaurants.GetRestaurantByOwnerIdAsync(GetCurrentUserId());
            if (restaurant == null) return NotFound();

            var orders = (await _unitOfWork.Orders.GetAllAsync())
                .Where(o => o.RestaurantId == restaurant.Id && o.Status != "Delivered" && o.Status != "Cancelled")
                .ToList();

            ViewBag.RestaurantId = restaurant.Id;
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _unitOfWork.CompleteAsync();

            await _orderHub.Clients.Group($"Order_{orderId}").SendAsync("ReceiveStatusUpdate", orderId.ToString(), status);

            if (status == "ReadyForPickup")
            {
                await _orderHub.Clients.Group("Drivers").SendAsync("ReceiveNewJob", new { orderId = order.Id, restaurantName = "رستوران دانیال" });
            }

            return Json(new { success = true });
        }

        public async Task<IActionResult> MenuManagement()
        {
            var restaurant = await _unitOfWork.Restaurants.GetRestaurantByOwnerIdAsync(GetCurrentUserId());
            if (restaurant == null) return NotFound();

            var foods = (await _unitOfWork.Foods.GetAllAsync()).Where(f => f.RestaurantId == restaurant.Id).ToList();
            return View(foods);
        }

        public async Task<IActionResult> AddFood() => View(new Food());

        [HttpPost]
        public async Task<IActionResult> AddFood(Food food)
        {
            var restaurant = await _unitOfWork.Restaurants.GetRestaurantByOwnerIdAsync(GetCurrentUserId());
            if (restaurant == null) return NotFound();

            food.RestaurantId = restaurant.Id;
            await _unitOfWork.Foods.AddAsync(food);
            await _unitOfWork.CompleteAsync();

            InvalidateCache(restaurant.Id);
            return RedirectToAction(nameof(MenuManagement));
        }

        public async Task<IActionResult> FoodOptions(int foodId)
        {
            var options = (await _unitOfWork.FoodOptions.GetAllAsync()).Where(o => o.FoodId == foodId).ToList();
            ViewBag.FoodId = foodId;
            return View(options);
        }

        [HttpPost]
        public async Task<IActionResult> AddOption(FoodOption option)
        {
            await _unitOfWork.FoodOptions.AddAsync(option);
            await _unitOfWork.CompleteAsync();

            var food = await _unitOfWork.Foods.GetByIdAsync(option.FoodId);
            if (food != null) InvalidateCache(food.RestaurantId);

            return RedirectToAction(nameof(FoodOptions), new { foodId = option.FoodId });
        }

        private void InvalidateCache(int restaurantId)
        {
            _cache.Remove("AllRestaurants");
            _cache.Remove("RestaurantMenu_" + restaurantId);
        }
    }
}
