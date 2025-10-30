using ApartmentManagement.Models;
using ApartmentManagement.Repositories.Interfaces;
using ApartmentManagement.Services;
using MongoDB.Driver;
using static System.Net.Mime.MediaTypeNames;

namespace ApartmentManagement.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly IMongoCollection<Room> _rooms;
        private readonly IMongoCollection<Apartment> _apartments;
        private readonly CloudService _cloudService;

        public RoomRepository(IMongoDBService mongoDBService, CloudService cloudService)
        {
            _rooms = mongoDBService.GetCollection<Room>("Rooms");
            _apartments = mongoDBService.GetCollection<Apartment>("Apartments");
            _cloudService = cloudService;
        }

        public Task<List<Room>> GetRoomsByApartmentId(string apartmentId)
        {
            var rooms = _rooms.Find(r => r.ApartmentId == apartmentId).ToListAsync();

            return rooms;
        }

        public Task<Room> GetRoomById(string id)
        {
            var room = _rooms.Find(r => r.Id == id).FirstOrDefaultAsync();

            return room;
        }

        public async Task<bool> CreateRoom(Room room, IFormFile[] images)
        {
            var imageUrls = await _cloudService.UploadRoomImgs(images, room.RoomName);
            room.ImageUrls = imageUrls;
            room.Status = "Trống";

            await _rooms.InsertOneAsync(room);

            var rooms = _rooms.Find(r => r.ApartmentId == room.ApartmentId).ToList();
            var apartment = _apartments.Find(a => a.Id == room.ApartmentId).FirstOrDefault();

            if (apartment != null)
            {
                apartment.Rooms = rooms;
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateRoom(Room room, IFormFile[] images)
        {
            var existingRoom = await _rooms.Find(r => r.Id == room.Id).FirstOrDefaultAsync();
            if (existingRoom == null)
            {
                return false;
            }

            // Xử lý ảnh nếu có upload mới
            if (images != null && images.Length > 0)
            {
                foreach (var image in existingRoom.ImageUrls ?? new List<string>())
                {
                    var deleted = await _cloudService.DeleteRoomImg(image, existingRoom.RoomName);
                    if (!deleted)
                    {
                        return false;
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
            return true;
        }

        public async Task<bool> DeleteRoom(string id)
        {
            var room = await _rooms.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (room == null)
            {
                return false;
            }
            foreach (var image in room.ImageUrls ?? new List<string>())
            {
                var deleted = await _cloudService.DeleteRoomImg(image, room.RoomName);
                if (!deleted)
                {
                    return false;
                }
            }
            await _rooms.DeleteOneAsync(r => r.Id == id);

            return true;
        }
    }
}
