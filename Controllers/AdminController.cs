using ApartmentManagement.Models;
using ApartmentManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Net.Http;
using System.Text.Json;
using System.Xml;

namespace ApartmentManagement.Controllers
{
    [Authorize(AuthenticationSchemes = "MyCookieAuth", Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Room> _rooms;
        private readonly CloudService _cloudService;
        private readonly HttpClient _httpClient;

        public AdminController(IMongoDBService mongoDBService, CloudService cloudService, HttpClient httpClient)
        {
            _users = mongoDBService.GetCollection<User>("Users");
            _rooms = mongoDBService.GetCollection<Room>("Rooms");
            _cloudService = cloudService;
            _httpClient = httpClient;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Account()
        {
            var user = _users.Find(user => true).ToList();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole([FromBody] Dictionary<string, string> data)
        {
            if (!data.ContainsKey("id") || !data.ContainsKey("role"))
                return BadRequest("Thiếu dữ liệu");

            var id = data["id"];
            var newRole = data["role"];

            var user = _users.Find(u => u.Id == id).FirstOrDefault();
            if (user == null) return NotFound();

            user.Role = newRole;
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

            return Ok();
        }

        [HttpGet]
        public IActionResult Room()
        {
            var rooms = _rooms.Find(room => true).ToList();
            return View(rooms);
        }

        [HttpGet]
        public IActionResult CreateRoom()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(Room room, IFormFile[] images)
        {
            // Gọi API để lấy tên tỉnh từ provinceCode
            var provinceResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/p/{room.Province}");
            var provinceData = JsonSerializer.Deserialize<JsonElement>(await provinceResponse.Content.ReadAsStringAsync());
            var provinceName = provinceData.GetProperty("name").GetString();

            // Gọi API để lấy tên quận từ districtCode
            var districtResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/d/{room.District}");
            var districtData = JsonSerializer.Deserialize<JsonElement>(await districtResponse.Content.ReadAsStringAsync());
            var districtName = districtData.GetProperty("name").GetString();

            var imageUrls = await _cloudService.UploadRoomImgs(images, room.RoomName);
            room.ImageUrls = imageUrls;
            room.Status = "Trống";

            room.Province = provinceName;
            room.District = districtName;

            await _rooms.InsertOneAsync(room);

            return RedirectToAction("Room");
        }
    }
}
