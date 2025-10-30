using ApartmentManagement.Models;
using ApartmentManagement.Repositories.Interfaces;
using ApartmentManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly Jwt _jwt;

        public AccountController(IAccountRepository accountRepository, Jwt jwt)
        {
            _accountRepository = accountRepository;
            _jwt = jwt;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User user, string confirmPass)
        {
            var (success, message) = await _accountRepository.Register(user, confirmPass);
            if (!success)
            {
                TempData["ErrorMessage"] = message ?? "Registration failed.";
                return RedirectToAction("Register");
            }

            TempData["SuccessMessage"] = "Registration successful.";

            return RedirectToAction("Login");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            var user = await _accountRepository.Login(email, password);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Login failed.";
                return View();
            }

            if (user.Status == true)
            {
                return RedirectToAction("ForceChangePassword", new { id = user.Id, rememberMe});
            }

            var authProps = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(3) : DateTimeOffset.UtcNow.AddHours(1)
            };

            var principal = _jwt.CreatePrincipal(user);
            await HttpContext.SignInAsync("MyCookieAuth", principal, authProps);
            return RedirectToAction("Index", "Room");
        }

        [HttpGet]
        public async Task<IActionResult> ForceChangePassword(string id, bool rememberMe)
        {
            var user = await _accountRepository.GetAccountById(id);
            ViewBag.RememberMe = rememberMe;

            return View("ForceChangePassword", user);
        }

        [HttpPost]
        [ActionName("ForceChangePasswordPost")]
        public async Task<IActionResult> ForceChangePasswordPost(string id, string newPassword, string confirmPassword, bool rememberMe)
        {
            var user = await _accountRepository.GetAccountById(id);

            await _accountRepository.ForceChangePassword(id, newPassword, confirmPassword);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(3) : DateTimeOffset.UtcNow.AddHours(1)
            };

            var principal = _jwt.CreatePrincipal(user);
            await HttpContext.SignInAsync("MyCookieAuth", principal, authProps);

            return RedirectToAction("Index", "Room");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgetPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            var success = await _accountRepository.ForgetPassword(email);
            if (!success)
            {
                TempData["ErrorMessage"] = "Password reset failed.";
                return View();
            }

            TempData["SuccessMessage"] = "A new password has been sent to your email!";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string id)
        {
            var user = await _accountRepository.GetProfile(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "Room");
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(string id, string fullName, string phoneNumber, string newEmail)
        {
            var (updatedUser, emailChanged) = await _accountRepository.UpdateAccount(id, fullName, phoneNumber, newEmail);

            if (updatedUser == null)
            {
                TempData["ErrorMessage"] = "An error occurred.";
                return RedirectToAction("Index", "Room");
            }

            if (emailChanged)
            {
                var currentAuth = await HttpContext.AuthenticateAsync("MyCookieAuth");
                var authProps = new AuthenticationProperties
                {
                    IsPersistent = currentAuth.Properties?.IsPersistent ?? false,
                    ExpiresUtc = currentAuth.Properties?.ExpiresUtc ?? DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync("MyCookieAuth", _jwt.CreatePrincipal(updatedUser), authProps);
            }

            TempData["SuccessMessage"] = "Your information has been updated!";
            return RedirectToAction("Profile", new { id });
        }

        public async Task<IActionResult> Avatar(string id)
        {
            var user = await _accountRepository.GetProfile(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "Room");
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(string id, IFormFile avatarFile)
        {
            var avatarUrl = await _accountRepository.UploadAvatar(id, avatarFile);
            if (avatarUrl == null)
            {
                TempData["ErrorMessage"] = "Failed to update avatar!";
                return RedirectToAction("Avatar", new { id });
            }

            var user = await _accountRepository.GetProfile(id);
            if (user != null)
            {
                var currentAuth = await HttpContext.AuthenticateAsync("MyCookieAuth");
                var authProps = new AuthenticationProperties
                {
                    IsPersistent = currentAuth.Properties?.IsPersistent ?? false,
                    ExpiresUtc = currentAuth.Properties?.ExpiresUtc ?? DateTimeOffset.UtcNow.AddHours(1)
                };
                await HttpContext.SignInAsync("MyCookieAuth", _jwt.CreatePrincipal(user), authProps);
            }

            TempData["SuccessMessage"] = "Avatar updated successfully.";
            return RedirectToAction("Profile", new { id });
        }

        public async Task<IActionResult> Password(string id)
        {
            var user = await _accountRepository.GetProfile(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "Room");
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Password(string id, string oldPassword, string newPassword, string confirmNewPassword)
        {
            var success = await _accountRepository.ChangePassword(id, oldPassword, newPassword, confirmNewPassword);
            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to update password!";
                var user = await _accountRepository.GetProfile(id);
                return View(user);
            }

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile", new { id });
        }
    }
}
