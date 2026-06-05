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
            ViewBag.TotalSales = orders.Sum(o => o.TotalAmount);
            ViewBag.OrderCount = orders.Count();
            ViewBag.PendingOrders = orders.Count(o => o.Status == "Pending");
            return View(orders);
        }

        public IActionResult KillSwitch()
        {
            ViewBag.IsActive = _killSwitch.IsSystemActive();
            return View();
        }

        [HttpPost]
        public IActionResult ToggleSystem(bool active)
        {
            _killSwitch.SetSystemStatus(active);
            return RedirectToAction(nameof(KillSwitch));
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
