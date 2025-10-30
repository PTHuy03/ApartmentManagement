using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApartmentManagement.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public User? User { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; } // "Invoice", "Contract", "Request"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
