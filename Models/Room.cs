using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApartmentManagement.Models
{
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ApartmentId { get; set; }
        public Apartment? Apartment { get; set; }
        public string RoomName { get; set; }
        public int RoomMember { get; set; }
        public int Area { get; set; }
        public List<string> ImageUrls { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Status { get; set; } = "Available"; // Available, Occupied, UnderMaintenance, NotAvailable
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
