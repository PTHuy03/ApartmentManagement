using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApartmentManagement.Models
{
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string RoomName { get; set; }
        public int RoomMember { get; set; }
        public int Area { get; set; }

        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string StreetAddress { get; set; }

        public List<string> ImageUrls { get; set; } // hoặc List<string> nếu có nhiều ảnh
        public string Description { get; set; }
        public double Price { get; set; }
        public string Status { get; set; }   // Trống / Đang thuê / Chờ duyệt

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
