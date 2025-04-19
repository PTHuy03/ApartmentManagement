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
        private const string DefaultAvatarFileName = "https://res.cloudinary.com/dpr5nrste/image/upload/v1744902717/ApartmentManagement/Avatar/AvatarDefualt.png";

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            if (!user.Avatar.Contains(DefaultAvatarFileName))
            {
                var deleteOldAvatar = await _cloudService.DeleteAvatar(user.Avatar);
                if (deleteOldAvatar == false)
                {
                    TempData["ErrorMessage"] = "Xóa ảnh thất bại.";
                    return RedirectToAction("Account");
                }
            }
            await _users.DeleteOneAsync(u => u.Id == id);
            return RedirectToAction("Account");
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

        [HttpGet]
        public IActionResult DetailRoom(string id)
        {
            var room = _rooms.Find(u => u.Id == id).FirstOrDefault();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoom(Room room, IFormFile[] images)
        {
            var existingRoom = await _rooms.Find(r => r.Id == room.Id).FirstOrDefaultAsync();
            if (existingRoom == null)
            {
                return NotFound();
            }

            // Cập nhật tên tỉnh/quận nếu có thay đổi mã
            if (existingRoom.Province != room.Province || existingRoom.District != room.District)
            {
                var provinceResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/p/{room.Province}");
                var provinceName = JsonSerializer.Deserialize<JsonElement>(await provinceResponse.Content.ReadAsStringAsync())
                                                    .GetProperty("name").GetString();

                var districtResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/d/{room.District}");
                var districtName = JsonSerializer.Deserialize<JsonElement>(await districtResponse.Content.ReadAsStringAsync())
                                                    .GetProperty("name").GetString();

                room.Province = provinceName;
                room.District = districtName;
            }

            // Xử lý ảnh nếu có upload mới
            if (images != null && images.Length > 0)
            {
                foreach (var image in existingRoom.ImageUrls ?? new List<string>())
                {
                    var deleted = await _cloudService.DeleteRoomImg(image, existingRoom.RoomName);
                    if (!deleted)
                    {
                        TempData["ErrorMessage"] = "Tải ảnh thất bại.";
                        return RedirectToAction("DetailRoom", new { id = room.Id });
                    }
                }

                var newUrls = await _cloudService.UploadRoomImgs(images, room.RoomName);
                room.ImageUrls = newUrls;
            }
            else
            {
                room.ImageUrls = existingRoom.ImageUrls;
            }

            room.CreatedAt = existingRoom.CreatedAt;
            room.Status = existingRoom.Status;

            await _rooms.ReplaceOneAsync(r => r.Id == room.Id, room);
            return RedirectToAction("DetailRoom", new { id = room.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            var room = await _rooms.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (room == null)
            {
                return NotFound();
            }
            foreach (var image in room.ImageUrls ?? new List<string>())
            {
                var deleted = await _cloudService.DeleteRoomImg(image, room.RoomName);
                if (!deleted)
                {
                    TempData["ErrorMessage"] = "Xóa ảnh thất bại.";
                    return RedirectToAction("DetailRoom", new { id = room.Id });
                }
            }
            await _rooms.DeleteOneAsync(r => r.Id == id);
            return RedirectToAction("Room");
        }
    }
}
