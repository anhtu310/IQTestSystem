using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Models.ViewModels;
using Project.Service;

namespace Project.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserSerivce userSerivce;
        private readonly IqtestSystemContext context;
        private readonly ILogger<AuthenticationController> logger;
        private readonly IConfiguration configuration;

        public AuthenticationController(IUserSerivce userSerivce, IqtestSystemContext context, IConfiguration configuration)
        {
            this.userSerivce = userSerivce;
            this.context = context;
            this.configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            Debug.WriteLine(User);
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminEmail = configuration["AdminAccount:Email"];
            var adminPassword = configuration["AdminAccount:Password"];
            if (model.Email == adminEmail && model.Password == adminPassword)
            {
                await SignInUser(model.Email, true, "0", "Admin");
                return RedirectToAction("Index", "Admin");
            }

            var user = await userSerivce.ValidateUser(model.Email, model.Password);

            if (user != null)
            {
                await SignInUser(user.Email, user.IsAdmin, user.UserId.ToString(), user.Username);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        private async Task SignInUser(string email, bool isAdmin, string userId, string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User"),
                new Claim("UserId", userId),
                new Claim("UserName", userName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                IsPersistent = false
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View("Register","Authentication");
        }

        [HttpPost]
        public IActionResult RegisterAuthen(string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu không giống nhau";
                return View("Register");
            }

            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                ViewBag.Error = "Email đã tồn tại";
                return View("Register");
            }

            string userName = email.Split('@')[0];

            var newUser = new User
            {
                Email = email,
                PasswordHash = HashPassword(password),
                Username = userName,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow,
                Status = true
            };

            context.Users.Add(newUser);
            context.SaveChanges();
            ViewBag.Success = "Đăng ký thành công! Mời bạn đăng nhập.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Authentication");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
