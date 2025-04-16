using MongoDB.Driver;
namespace ApartmentManagement.Services
{
    public interface IMongoDBService
    {
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}
