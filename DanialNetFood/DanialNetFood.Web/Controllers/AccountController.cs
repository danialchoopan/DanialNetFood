using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using DanialNetFood.Web.Data.UnitOfWork;
using DanialNetFood.Web.Models;

namespace DanialNetFood.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = (await _unitOfWork.Users.FindAsync(u => u.Username == username)).FirstOrDefault();

            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    if (user.Role == "RestaurantOwner") return RedirectToAction("LiveOrders", "RestaurantAdmin");
                    if (user.Role == "SuperAdmin") return RedirectToAction("Dashboard", "SuperAdmin");
                    if (user.Role == "Driver") return RedirectToAction("Dashboard", "Driver");

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "نام کاربری یا رمز عبور اشتباه است");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
