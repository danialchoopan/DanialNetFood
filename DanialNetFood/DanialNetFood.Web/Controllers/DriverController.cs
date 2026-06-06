using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DanialNetFood.Web.Data.UnitOfWork;
using DanialNetFood.Web.Models;
using Microsoft.AspNetCore.SignalR;
using DanialNetFood.Web.Hubs;
using DanialNetFood.Web.Services;

namespace DanialNetFood.Web.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<OrderHub> _orderHub;
        private readonly IWalletService _walletService;

        public DriverController(IUnitOfWork unitOfWork, IHubContext<OrderHub> orderHub, IWalletService walletService)
        {
            _unitOfWork = unitOfWork;
            _orderHub = orderHub;
            _walletService = walletService;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        public async Task<IActionResult> Dashboard()
        {
            var availableOrders = (await _unitOfWork.Orders.GetAllAsync())
                .Where(o => o.Status == "ReadyForPickup" && o.DriverId == null)
                .ToList();

            var myActiveOrders = (await _unitOfWork.Orders.GetAllAsync())
                .Where(o => o.DriverId == GetCurrentUserId() && o.Status == "OutForDelivery")
                .ToList();

            ViewBag.ActiveOrders = myActiveOrders;
            return View(availableOrders);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null || order.Status != "ReadyForPickup") return BadRequest();

            order.DriverId = GetCurrentUserId();
            order.Status = "OutForDelivery";
            await _unitOfWork.CompleteAsync();

            await _orderHub.Clients.Group($"Order_{orderId}").SendAsync("ReceiveStatusUpdate", orderId.ToString(), "OutForDelivery");

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        public async Task<IActionResult> DeliverOrder(int orderId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null || order.DriverId != GetCurrentUserId()) return BadRequest();

            await _unitOfWork.BeginTransactionAsync();
            try {
                order.Status = "Delivered";

                // Payouts
                var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(order.RestaurantId);
                if (restaurant != null) {
                    var restaurantShare = order.TotalAmount - order.CommissionAmount;
                    await _walletService.CreditAsync(restaurant.OwnerId, restaurantShare, $"سهم فروش سفارش #{orderId}");
                }

                // Driver fee (Fixed 10000 for demo)
                await _walletService.CreditAsync(GetCurrentUserId(), 10000, $"کارمزد تحویل سفارش #{orderId}");

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _orderHub.Clients.Group($"Order_{orderId}").SendAsync("ReceiveStatusUpdate", orderId.ToString(), "Delivered");
                return RedirectToAction(nameof(Dashboard));
            } catch {
                await _unitOfWork.RollbackTransactionAsync();
                return View("Error");
            }
        }
    }
}
