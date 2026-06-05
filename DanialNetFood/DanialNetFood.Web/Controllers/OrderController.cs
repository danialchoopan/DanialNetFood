using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using DanialNetFood.Web.Data.UnitOfWork;
using DanialNetFood.Web.Models;
using DanialNetFood.Web.Models.ViewModels;
using DanialNetFood.Web.Services.DiscountStrategy;
using DanialNetFood.Web.Hubs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DanialNetFood.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderController(IUnitOfWork unitOfWork, IHubContext<OrderHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public IActionResult Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Items.Any()) return RedirectToAction("Index", "Home");
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyDiscount(string code)
        {
            var cart = GetCartFromSession();
            var discount = (await _unitOfWork.DiscountCodes.FindAsync(d => d.Code == code && d.IsActive)).FirstOrDefault();

            if (discount != null)
            {
                IDiscountStrategy strategy = discount.Type == "Percentage"
                    ? new PercentageDiscountStrategy()
                    : new FixedAmountDiscountStrategy();

                var discountContext = new DiscountContext(strategy);
                var newTotal = discountContext.ExecuteStrategy(cart.TotalAmount, discount.Value);

                return Json(new { success = true, newTotal = newTotal, message = "تخفیف اعمال شد" });
            }

            return Json(new { success = false, message = "کد تخفیف معتبر نیست" });
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var cart = GetCartFromSession();
            if (!cart.Items.Any()) return RedirectToAction("Index", "Home");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0"),
                    TotalAmount = cart.TotalAmount,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    Items = cart.Items.Select(i => new OrderItem
                    {
                        FoodId = i.FoodId,
                        FoodName = i.FoodName,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                HttpContext.Session.Remove("UserCart");
                return RedirectToAction("Track", new { id = order.Id });
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return View("Error");
            }
        }

        public async Task<IActionResult> Track(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.CompleteAsync();
                await _hubContext.Clients.Group($"Order_{orderId}").SendAsync("ReceiveStatusUpdate", status);
                return Ok();
            }
            return NotFound();
        }

        private CartViewModel GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString("UserCart");
            return sessionData == null ? new CartViewModel() : JsonConvert.DeserializeObject<CartViewModel>(sessionData)!;
        }
    }
}
