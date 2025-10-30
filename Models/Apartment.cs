using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ApartmentManagement.Models
{
    public class Apartment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ApartmentName { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string StreetAddress { get; set; }
        public string CreatedBy { get; set; }
        public User? Admin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Room> Rooms { get; set; } = new List<Room>();
    }

}
