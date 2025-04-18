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
        private const string DefaultAvatarFileName = "https://res.cloudinary.com/dpr5nrste/image/upload/v1744902717/ApartmentManagement/Avatar/AvatarDefualt.png";

        public AccountController(IMongoDBService mongoDBService, CloudService cloudService, EmailSender emailSender)
        {
            _users = mongoDBService.GetCollection<User>("Users");
            _cloudService = cloudService;
            _emailSender = emailSender;
        }

        private ClaimsPrincipal CreatePrincipal(User user, string authScheme = "MyCookieAuth")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.UserData, user.Avatar)
            };
            var identity = new ClaimsIdentity(claims, authScheme);
            return new ClaimsPrincipal(identity);
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

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(3) : DateTimeOffset.UtcNow.AddHours(1)
            };

            var claimsPrincipal = CreatePrincipal(user);
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
        public IActionResult Profile(string id)
        {
            var user = _users.Find(u => u.Id == id).FirstOrDefault();
            if(user == null) 
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index", "Room");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(string id, string fullName, string phoneNumber, string newEmail)
        {
            var user = _users.Find(u => u.Id == id).FirstOrDefault();
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index", "Room");
            }
            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            if(user.Email != newEmail)
            {
                user.Email = newEmail;
                var currentAuthResult = await HttpContext.AuthenticateAsync("MyCookieAuth");
                var isPersistent = currentAuthResult.Properties?.IsPersistent ?? false;
                var expiresUtc = currentAuthResult.Properties?.ExpiresUtc ??
                    (isPersistent ? DateTimeOffset.UtcNow.AddDays(3) : DateTimeOffset.UtcNow.AddHours(1));

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = isPersistent,
                    ExpiresUtc = expiresUtc
                };

                var claimsPrincipal = CreatePrincipal(user);
                await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal, authProperties);
            }
            _users.ReplaceOne(u => u.Id == user.Id, user);
            TempData["SuccessMessage"] = "Cập nhật thành công";
            return RedirectToAction("Profile", "Account", new { email = user.Email });
        }

        [HttpGet]
        public IActionResult Avatar(string id)
        {
            var user = _users.Find(u => u.Id == id).FirstOrDefault();
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index", "Room");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(string id, IFormFile avatarFile)
        {
            var user = _users.Find(u => u.Id == id).FirstOrDefault();
            if (user == null || avatarFile == null || avatarFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Tải ảnh thất bại.";
                return RedirectToAction("Avatar", new { id });
            }

            try
            {
                var avatarUrl = await _cloudService.UploadAvatar(avatarFile);
                if (!user.Avatar.Contains(DefaultAvatarFileName))
                {
                    var deleteOldAvatar = await _cloudService.DeleteAvatar(user.Avatar);
                    if (deleteOldAvatar == false)
                    {
                        TempData["ErrorMessage"] = "Tải ảnh thất bại.";
                        return RedirectToAction("Avatar", new { id });
                    }
                }
                user.Avatar = avatarUrl;

                var currentAuthResult = await HttpContext.AuthenticateAsync("MyCookieAuth");
                var isPersistent = currentAuthResult.Properties?.IsPersistent ?? false;
                var expiresUtc = currentAuthResult.Properties?.ExpiresUtc ??
                    (isPersistent ? DateTimeOffset.UtcNow.AddDays(3) : DateTimeOffset.UtcNow.AddHours(1));

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = isPersistent,
                    ExpiresUtc = expiresUtc
                };

                var claimsPrincipal = CreatePrincipal(user);
                await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal, authProperties);

                _users.ReplaceOne(u => u.Id == user.Id, user);
                TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải ảnh lên: " + ex.Message;
            }

            return RedirectToAction("Profile", "Account", new { id });
        }

        [HttpGet]
        public IActionResult Password(string id)
        {
            var user = _users.Find(u => u.Id == id).FirstOrDefault();
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index", "Room");
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult Password(string id, string oldPassword, string newPassword, string confirmNewPassword)
        {
            var user = _users.Find(u => u.Id == id).FirstOrDefault();
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index", "Room");
            }
            if (user.PasswordHash != HashPassword(oldPassword))
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng");
                return View(user);
            }
            if (newPassword != confirmNewPassword)
            {
                ModelState.AddModelError("ConfirmNewPassword", "Mật khẩu xác nhận không khớp");
                return View(user);
            }
            user.PasswordHash = HashPassword(newPassword);
            _users.ReplaceOne(u => u.Id == user.Id, user);
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công";
            return RedirectToAction("Profile", "Account", new { id = user.Id });
        }
    }
}
