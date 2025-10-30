using ApartmentManagement.Models;

namespace ApartmentManagement.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<List<Room>> GetRoomsByApartmentId(string apartmentId);
        Task<Room> GetRoomById(string id);
        Task<bool> CreateRoom(Room room, IFormFile[] images);
        Task<bool> UpdateRoom(Room room, IFormFile[] images);
        Task<bool> DeleteRoom(string id);
    }
}
