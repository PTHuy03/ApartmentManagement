using ApartmentManagement.Models;
using ApartmentManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApartmentManagement.Controllers
{
    public class AccountController : Controller
    {

        private readonly IMongoCollection<User> _users;
        private readonly CloudService _cloudService;

        public AccountController(IMongoDBService mongoDBService, CloudService cloudService)
        {
            _users = mongoDBService.GetCollection<User>("Users");
            _cloudService = cloudService;
        }
        [HttpGet]
        public IActionResult Register() => View();
        [HttpPost]
        public IActionResult Register(User user, string confirmPass)
        {
            var existingUser = _users.Find(u => u.Email == user.Email).FirstOrDefault();
            if(existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại");
                return View(user);
            }
            if(user.PasswordHash != confirmPass)
            {
                ModelState.AddModelError("ConfirmPass", "Mật khẩu xác nhận không khớp");
            }
            
            user.PasswordHash = HashPassword(user.PasswordHash);
            user.Role = "Tenant";
            user.Avatar = "https://res.cloudinary.com/dpr5nrste/image/upload/v1744902717/ApartmentManagement/Avatar/AvatarDefualt.png";
            _users.InsertOne(user);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login() => View();
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            var user = _users.Find(u => u.Email == email).FirstOrDefault();
            if (user == null || user.PasswordHash != HashPassword(password))
            {
                ModelState.AddModelError(string.Empty, "Sai thông tin đăng nhập");
                return View();
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(3) : DateTimeOffset.UtcNow.AddHours(1)
            };

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal, authProperties);

            return RedirectToAction("Index", "Room");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
