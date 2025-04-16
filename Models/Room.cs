using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApartmentManagement.Models
{
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public string RoomName { get; set; }
        public int RoomMember { get; set; }
        public int Area { get; set; }
        public string Address { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Status { get; set; }
    }
}
