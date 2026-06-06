using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DanialNetFood.Web.Models.ViewModels;
using DanialNetFood.Web.Data.UnitOfWork;

namespace DanialNetFood.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string CartSessionKey = "UserCart";

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int foodId, List<int> optionIds)
        {
            var food = await _unitOfWork.Foods.GetByIdAsync(foodId);
            if (food == null) return NotFound();

            var cart = GetCartFromSession();

            var cartItemOptions = new List<CartItemOption>();
            if (optionIds != null)
            {
                foreach (var optId in optionIds)
                {
                    var opt = await _unitOfWork.FoodOptions.GetByIdAsync(optId);
                    if (opt != null)
                    {
                        cartItemOptions.Add(new CartItemOption { Id = opt.Id, Name = opt.Name, Price = opt.Price });
                    }
                }
            }

            // Create a sorted list of IDs to use as a key for item differentiation
            var sortedOptionIds = cartItemOptions.Select(o => o.Id).OrderBy(id => id).ToList();

            var existingItem = cart.Items.FirstOrDefault(i => i.FoodId == foodId &&
                i.Options.Select(o => o.Id).OrderBy(id => id).SequenceEqual(sortedOptionIds));

            if (existingItem == null)
            {
                cart.Items.Add(new CartItem {
                    FoodId = food.Id,
                    FoodName = food.Name,
                    Price = food.Price,
                    Quantity = 1,
                    Options = cartItemOptions
                });
            }
            else
            {
                existingItem.Quantity++;
            }

            SaveCartToSession(cart);
            return PartialView("_CartPartial", cart);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int foodId, string? optionsHash)
        {
            var cart = GetCartFromSession();

            // If optionsHash is provided, match exactly. Otherwise (legacy), match foodId.
            CartItem? itemToRemove;
            if (!string.IsNullOrEmpty(optionsHash))
            {
                itemToRemove = cart.Items.FirstOrDefault(i =>
                    i.FoodId == foodId &&
                    string.Join(",", i.Options.Select(o => o.Id).OrderBy(id => id)) == optionsHash);
            }
            else
            {
                itemToRemove = cart.Items.FirstOrDefault(i => i.FoodId == foodId);
            }

            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
            }

            SaveCartToSession(cart);
            return PartialView("_CartPartial", cart);
        }

        private CartViewModel GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString(CartSessionKey);
            return sessionData == null ? new CartViewModel() : JsonConvert.DeserializeObject<CartViewModel>(sessionData)!;
        }

        private void SaveCartToSession(CartViewModel cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }
    }
}
