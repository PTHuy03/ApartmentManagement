using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ApartmentManagement.Models
{
    public class Request
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string RoomId { get; set; }
        public Room? Room { get; set; }
        public string TenantId { get; set; }
        public User? User { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Rejected
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public User? Admin { get; set; } 
    }
}
