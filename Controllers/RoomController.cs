using System.Diagnostics;
using ApartmentManagement.Models;
using ApartmentManagement.Repositories.Interfaces;
using ApartmentManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ApartmentManagement.Controllers
{
    public class RoomController : Controller
    {
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IRoomRepository _roomRepository;

        public RoomController(IApartmentRepository apartmentRepository, IRoomRepository roomRepository)
        {
            _apartmentRepository = apartmentRepository;
            _roomRepository = roomRepository;
        }

        public async Task<IActionResult> Index()
        {
            var apartments = await _apartmentRepository.GetAllApartments();

            foreach (var apartment in apartments)
            {
                apartment.Rooms = await _roomRepository.GetRoomsByApartmentId(apartment.Id);
            }

            return View(apartments);
        }

        public async Task<IActionResult> RoomDetail(string id)
        {
            var room = await _roomRepository.GetRoomById(id);
            var apartment = await _apartmentRepository.GetApartmentById(room.ApartmentId);
            var address = $"{apartment.StreetAddress}, {apartment.Ward}, {apartment.Province}";

            ViewBag.Address = address;

            return View(room);
        }
    }
}
