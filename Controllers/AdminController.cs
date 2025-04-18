using ApartmentManagement.Models;
using ApartmentManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ApartmentManagement.Controllers
{
    [Authorize(AuthenticationSchemes = "MyCookieAuth", Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Room> _rooms;

        public AdminController(IMongoDBService mongoDBService)
        {
            _users = mongoDBService.GetCollection<User>("Users");
            _rooms = mongoDBService.GetCollection<Room>("Rooms");
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
    }
}
