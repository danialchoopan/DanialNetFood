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
using DanialNetFood.Web.Services;

namespace DanialNetFood.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ILocationService _locationService;
        private readonly IPricingService _pricingService;
        private readonly IWalletService _walletService;
        private readonly IKillSwitchService _killSwitch;

        public OrderController(IUnitOfWork unitOfWork,
            IHubContext<OrderHub> hubContext,
            ILocationService locationService,
            IPricingService pricingService,
            IWalletService walletService,
            IKillSwitchService killSwitch)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _locationService = locationService;
            _pricingService = pricingService;
            _walletService = walletService;
            _killSwitch = killSwitch;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Items.Any()) return RedirectToAction("Index", "Home");

            if (!_killSwitch.IsSystemActive())
            {
                ViewBag.Error = "سامانه در حال حاضر در حالت به‌روزرسانی است.";
                return View(cart);
            }

            // Geofencing Check
            var firstItem = cart.Items.First();
            var food = await _unitOfWork.Foods.GetByIdAsync(firstItem.FoodId);
            if (food != null)
            {
                var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(food.RestaurantId);
                var userId = GetCurrentUserId();
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (restaurant != null && user != null && user.Latitude.HasValue && user.Longitude.HasValue)
                {
                    var distance = _locationService.CalculateDistance(user.Latitude.Value, user.Longitude.Value, restaurant.Latitude, restaurant.Longitude);
                    if (distance > restaurant.ServiceRadiusKm)
                    {
                        ViewBag.GeofenceError = "شما خارج از محدوده سرویس‌دهی این رستوران هستید.";
                    }
                }
            }

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

            // 1. Kill Switch Enforcement
            if (!_killSwitch.IsSystemActive())
            {
                ViewBag.Error = "ثبت سفارش به دلیل به‌روزرسانی سیستم موقتاً غیرفعال است.";
                return View("Checkout", cart);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userId = GetCurrentUserId();
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                var firstItem = cart.Items.First();
                var foodRef = await _unitOfWork.Foods.GetByIdAsync(firstItem.FoodId);
                var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(foodRef?.RestaurantId ?? 0);

                if (restaurant == null) throw new Exception("رستوران معتبر نیست.");

                // 2. Geofencing Enforcement (POST)
                if (user != null && user.Latitude.HasValue && user.Longitude.HasValue)
                {
                    var distance = _locationService.CalculateDistance(user.Latitude.Value, user.Longitude.Value, restaurant.Latitude, restaurant.Longitude);
                    if (distance > restaurant.ServiceRadiusKm)
                        throw new Exception("آدرس تحویل خارج از محدوده سرویس‌دهی رستوران است.");
                }

                var order = new Order
                {
                    UserId = userId,
                    RestaurantId = restaurant.Id,
                    TotalAmount = cart.TotalAmount,
                    CommissionAmount = _pricingService.CalculateCommission(cart.TotalAmount),
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    Items = cart.Items.Select(i => new OrderItem
                    {
                        FoodId = i.FoodId,
                        FoodName = i.FoodName,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        Options = i.Options.Select(o => new OrderItemOption {
                            FoodOptionId = o.Id,
                            OptionName = o.Name,
                            Price = o.Price
                        }).ToList()
                    }).ToList()
                };

                // 3. Wallet Deduction
                await _walletService.DebitAsync(userId, order.TotalAmount, $"برداشت برای سفارش #{order.Id}");

                // 4. Stock Check and Deduction (Optimistic Concurrency)
                foreach (var item in order.Items)
                {
                    var food = await _unitOfWork.Foods.GetByIdAsync(item.FoodId);
                    if (food == null || food.StockQuantity < item.Quantity)
                        throw new Exception($"موجودی غذای {item.FoodName} کافی نیست.");

                    food.StockQuantity -= item.Quantity;

                    foreach (var opt in item.Options)
                    {
                        var foodOpt = await _unitOfWork.FoodOptions.GetByIdAsync(opt.FoodOptionId);
                        if (foodOpt != null)
                        {
                            if (foodOpt.StockQuantity < item.Quantity)
                                throw new Exception($"موجودی {opt.OptionName} کافی نیست.");
                            foodOpt.StockQuantity -= item.Quantity;
                        }
                    }
                }

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                HttpContext.Session.Remove("UserCart");
                await _hubContext.Clients.Group($"Restaurant_{restaurant.Id}").SendAsync("ReceiveNewOrder", new { orderId = order.Id });

                return RedirectToAction("Track", new { id = order.Id });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                ViewBag.Error = ex.Message;
                return View("Checkout", cart);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null || order.Status != "Pending") return BadRequest();

            await _unitOfWork.BeginTransactionAsync();
            try {
                order.Status = "Cancelled";
                await _walletService.CreditAsync(order.UserId, order.TotalAmount, $"برگشت وجه سفارش لغو شده #{id}");

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _hubContext.Clients.Group($"Order_{id}").SendAsync("ReceiveStatusUpdate", id.ToString(), "Cancelled");
                return RedirectToAction("Track", new { id = id });
            } catch {
                await _unitOfWork.RollbackTransactionAsync();
                return View("Error");
            }
        }

        public async Task<IActionResult> Track(int id)
        {
            var order = (await _unitOfWork.Orders.GetAllAsync()).FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        private CartViewModel GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString("UserCart");
            return sessionData == null ? new CartViewModel() : JsonConvert.DeserializeObject<CartViewModel>(sessionData)!;
        }
    }
}
