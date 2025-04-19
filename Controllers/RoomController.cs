using System.Diagnostics;
using ApartmentManagement.Models;
using ApartmentManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ApartmentManagement.Controllers
{
    public class RoomController : Controller
    {
        private readonly IMongoCollection<Room> _rooms;

        public RoomController(IMongoDBService mongoDBService)
        {
            _rooms = mongoDBService.GetCollection<Room>("Rooms");
        }

        public IActionResult Index()
        {
            var rooms = _rooms.Find(room => true).ToList();
            return View(rooms);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
