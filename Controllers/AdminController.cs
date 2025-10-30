using ApartmentManagement.Models;
using ApartmentManagement.Repositories.Interfaces;
using ApartmentManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Net.Http;

namespace ApartmentManagement.Controllers
{
    [Authorize(AuthenticationSchemes = "MyCookieAuth", Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly CloudService _cloudService;
        private readonly HttpClient _httpClient;

        private const string DefaultAvatarFileName = "https://res.cloudinary.com/dpr5nrste/image/upload/v1744902717/ApartmentManagement/Avatar/AvatarDefualt.png";

        public AdminController(
            CloudService cloudService,
            HttpClient httpClient,
            IAccountRepository accountRepository,
            IApartmentRepository apartmentRepository,
            IRoomRepository roomRepository)
        {
            _accountRepository = accountRepository;
            _apartmentRepository = apartmentRepository;
            _roomRepository = roomRepository;
            _cloudService = cloudService;
            _httpClient = httpClient;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var users = await _accountRepository.GetAllAccounts();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole([FromBody] dynamic data)
        {
            string id = data.GetProperty("id").GetString();
            string role = data.GetProperty("role").GetString();

            var success = await _accountRepository.ChangeRole(id, role);
            if (!success)
            {
                return BadRequest("An error occurred.");
            }

            return Ok("Role updated successfully.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var success = await _accountRepository.DeleteAccount(id);
            if (!success)
            {
                TempData["ErrorMessage"] = "An error occurred.";
                return RedirectToAction("Account");
            }

            return RedirectToAction("Account");
        }

        [HttpGet]
        public async Task<IActionResult> Apartment()
        {
            var adminId = User.FindFirst("UserId")?.Value;
            var apartments = await _apartmentRepository.GetApartmentsByAdminId(adminId);

            if (apartments == null)
            {
                TempData["ErrorMessage"] = "No apartments found.";
                return View(apartments);
            }

            return View(apartments);
        }

        [HttpGet]
        public IActionResult CreateApartment()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateApartment(Apartment apartment)
        {
            var adminId = User.FindFirst("UserId")?.Value;
            _apartmentRepository.CreateApartment(apartment, adminId);

            return RedirectToAction("Apartment");
        }

        [HttpGet]
        public async Task<IActionResult> ApartmentDetail(string id)
        {
            var apartment = await _apartmentRepository.GetApartmentById(id);
            var rooms = await _roomRepository.GetRoomsByApartmentId(id);

            ViewBag.Apartment = apartment;

            return View(rooms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditApartment(Apartment apartment)
        {
            var result = await _apartmentRepository.UpdateApartment(apartment);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to update apartment.";
                return RedirectToAction("ApartmentDetail", new { id = apartment.Id });
            }

            return RedirectToAction("ApartmentDetail", new { id = apartment.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteApartment(string id)
        {
            var result = await _apartmentRepository.DeleteApartment(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to delete apartment.";
                return RedirectToAction("ApartmentDetail", new { id });
            }

            return RedirectToAction("Apartment");
        }

        [HttpGet]
        public IActionResult CreateRoom(string apartmentId)
        {
            ViewBag.ApartId = apartmentId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(Room room, IFormFile[] images)
        {
            var result = await _roomRepository.CreateRoom(room, images);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to create room.";
                return RedirectToAction("ApartmentDetail", new { id = room.ApartmentId });
            }

            return RedirectToAction("ApartmentDetail", new { id = room.ApartmentId });
        }

        [HttpGet]
        public async Task<IActionResult> DetailRoom(string id)
        {
            var room = await _roomRepository.GetRoomById(id);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoom(Room room, IFormFile[] images)
        {
            var result = await _roomRepository.UpdateRoom(room, images);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to update room.";
                return RedirectToAction("DetailRoom", new { id = room.Id });
            }

            return RedirectToAction("ApartmentDetail", new { id = room.ApartmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            var result = await _roomRepository.DeleteRoom(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to delete room.";
                return RedirectToAction("DetailRoom", new { id });
            }

            return RedirectToAction("Room");
        }
    }
}
