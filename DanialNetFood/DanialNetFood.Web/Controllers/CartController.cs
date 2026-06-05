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
        public async Task<IActionResult> AddToCart(int foodId)
        {
            var food = await _unitOfWork.Foods.GetByIdAsync(foodId);
            if (food == null) return NotFound();

            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.FoodId == foodId);
            if (item == null)
            {
                cart.Items.Add(new CartItem { FoodId = food.Id, FoodName = food.Name, Price = food.Price, Quantity = 1 });
            }
            else
            {
                item.Quantity++;
            }

            SaveCartToSession(cart);
            return PartialView("_CartPartial", cart);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int foodId)
        {
            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.FoodId == foodId);
            if (item != null)
            {
                cart.Items.Remove(item);
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
