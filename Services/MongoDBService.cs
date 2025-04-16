using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace ApartmentManagement.Services
{
    public class MongoDBService : IMongoDBService
    {
        private readonly IMongoDatabase _database;
        public MongoDBService(IConfiguration configuration)
        {
            var mongoDBSection = configuration.GetSection("MongoDB");
            var client = new MongoClient(mongoDBSection["MongoDBString"]);
            _database = client.GetDatabase(mongoDBSection["DatabaseName"]);
        }
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}
