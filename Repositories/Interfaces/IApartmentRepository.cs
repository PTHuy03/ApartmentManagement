using ApartmentManagement.Models;
namespace ApartmentManagement.Repositories.Interfaces
{
    public interface IApartmentRepository
    {
        Task<List<Apartment>> GetAllApartments();
        Task<List<Apartment>> GetApartmentsByAdminId(string adminId);
        Task<Apartment> GetApartmentById(string id);
        void CreateApartment(Apartment apartment, string adminId);
        Task<bool> UpdateApartment(Apartment apartment);
        Task<bool> DeleteApartment(string id);
    }
}
