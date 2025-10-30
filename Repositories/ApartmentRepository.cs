using ApartmentManagement.Models;
using ApartmentManagement.Repositories.Interfaces;
using ApartmentManagement.Services;
using MongoDB.Driver;
using System.Net.Http;
using System.Text.Json;

namespace ApartmentManagement.Repositories
{
    public class ApartmentRepository : IApartmentRepository
    {
        private readonly IMongoCollection<Apartment> _apartments;
        private readonly HttpClient _httpClient;

        public ApartmentRepository(IMongoDBService mongoDBService, HttpClient httpClient)
        {
            _apartments = mongoDBService.GetCollection<Apartment>("Apartments");
            _httpClient = httpClient;
        }

        public async Task<List<Apartment>> GetAllApartments()
        {
            var apartments = await _apartments.Find(_ => true).ToListAsync();

            return apartments;
        }

        public async Task<List<Apartment>> GetApartmentsByAdminId(string adminId)
        {
            var apartments = await _apartments.Find(a => a.CreatedBy == adminId).ToListAsync();

            return apartments;
        }

        public async Task<Apartment> GetApartmentById(string id)
        {
            var apartment = await _apartments.Find(a => a.Id == id).FirstOrDefaultAsync();

            return apartment;
        }

        public async void CreateApartment(Apartment apartment, string adminId)
        {
            // Gọi API để lấy tên tỉnh từ provinceCode
            var provinceResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/p/{apartment.Province}");
            var provinceData = JsonSerializer.Deserialize<JsonElement>(await provinceResponse.Content.ReadAsStringAsync());
            var provinceName = provinceData.GetProperty("name").GetString();

            // Gọi API để lấy tên quận từ districtCode
            var districtResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/d/{apartment.District}");
            var districtData = JsonSerializer.Deserialize<JsonElement>(await districtResponse.Content.ReadAsStringAsync());
            var districtName = districtData.GetProperty("name").GetString();
            apartment.Province = provinceName;
            apartment.District = districtName;

            apartment.CreatedBy = adminId;

            await _apartments.InsertOneAsync(apartment);
        }

        public async Task<bool> UpdateApartment(Apartment apartment)
        {
            var existingApart = await _apartments.Find(r => r.Id == apartment.Id).FirstOrDefaultAsync();
            if (existingApart == null)
            {
                return false;
            }

            var provinceResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/p/{apartment.Province}");
            var provinceName = JsonSerializer.Deserialize<JsonElement>(await provinceResponse.Content.ReadAsStringAsync())
                                             .GetProperty("name").GetString();

            var districtResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/d/{apartment.District}");
            var districtName = JsonSerializer.Deserialize<JsonElement>(await districtResponse.Content.ReadAsStringAsync())
                                             .GetProperty("name").GetString();

            apartment.Province = provinceName;
            apartment.District = districtName;
            apartment.CreatedBy = existingApart.CreatedBy;
            await _apartments.ReplaceOneAsync(a => a.Id == apartment.Id, apartment);

            return true;
        }

        public async Task<bool> DeleteApartment(string id)
        {
            var existingApart = await _apartments.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (existingApart == null)
            {
                return false;
            }

            await _apartments.DeleteOneAsync(a => a.Id == id);

            return true;
        }
    }
}
