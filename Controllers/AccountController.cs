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
        private readonly EmailSender _emailSender;

        public AccountController(IMongoDBService mongoDBService, CloudService cloudService, EmailSender emailSender)
        {
            _users = mongoDBService.GetCollection<User>("Users");
            _cloudService = cloudService;
            _emailSender = emailSender;
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
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.UserData, user.Avatar)
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

        [HttpGet]
        public IActionResult ForgetPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            var user = _users.Find(u => u.Email == email).FirstOrDefault();
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email không tồn tại");
                return View();
            }

            var newPassword = Guid.NewGuid().ToString("N").Substring(0, 8); // Tạo mật khẩu mới ngẫu nhiên
            user.PasswordHash = HashPassword(newPassword);
            _users.ReplaceOne(u => u.Id == user.Id, user);

            // Gửi mật khẩu mới qua email
            var subject = "Mật khẩu mới của bạn";
            var body = $@"
                <p>Chào <strong>{user.FullName}</strong>,</p>
                <p>Mật khẩu mới của bạn là: <strong>{newPassword}</strong></p>
                <p>Vui lòng đăng nhập và thay đổi mật khẩu ngay.</p>
            ";

            await _emailSender.SendEmailAsync(email, subject, body);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Profile(string email)
        {
            var user = _users.Find(u => u.Email == email).FirstOrDefault();
            if(user == null) 
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index", "Room");
            }

            return View(user);
        }

    }
}
