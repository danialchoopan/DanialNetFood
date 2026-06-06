using Microsoft.AspNetCore.Mvc;
using DanialNetFood.Web.Models.ViewModels;
using Newtonsoft.Json;

namespace DanialNetFood.Web.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new CartViewModel()
                : JsonConvert.DeserializeObject<CartViewModel>(cartJson);

            return View(cart);
        }
    }
}
