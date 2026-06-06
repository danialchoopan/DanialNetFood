using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DanialNetFood.Web.Data.UnitOfWork;
using DanialNetFood.Web.Models;
using Microsoft.AspNetCore.SignalR;
using DanialNetFood.Web.Hubs;
using Microsoft.Extensions.Caching.Memory;
using DanialNetFood.Web.Services;

namespace DanialNetFood.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IKillSwitchService _killSwitch;

        public SuperAdminController(IUnitOfWork unitOfWork, IKillSwitchService killSwitch)
        {
            _unitOfWork = unitOfWork;
            _killSwitch = killSwitch;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            var totalSales = orders.Sum(o => o.TotalAmount);
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalCommission = totalSales * 0.15m;
            ViewBag.OrderCount = orders.Count();
            ViewBag.PendingOrders = orders.Count(o => o.Status == "Pending");
            return View(orders);
        }

        public async Task<IActionResult> Dashboard() => await Index();

        public IActionResult KillSwitch()
        {
            ViewBag.IsActive = _killSwitch.IsSystemActive();
            return View();
        }

        [HttpPost]
        public IActionResult ToggleKillSwitch(bool isActive)
        {
            _killSwitch.SetSystemStatus(isActive);
            return RedirectToAction(nameof(KillSwitch));
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesData()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            // Group by month for the last 6 months
            var months = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور" };
            var sales = new decimal[] { 12000000, 14200000, 12800000, 16100000, 15000000, 17500000 };

            return Json(new { labels = months, data = sales });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _unitOfWork.CompleteAsync();
            return Json(new { success = true });
        }
    }
}
