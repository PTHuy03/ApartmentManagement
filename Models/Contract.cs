using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApartmentManagement.Models
{
    public class Contract
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string RoomId { get; set; }
        public Room? Room { get; set; }
        public string tenantId { get; set; }
        public User? User { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Desposit { get; set; }
        public int ElectricStart { get; set; }
        public int WaterStart { get; set; }
        public decimal WaterRatePerUnit { get; set; }
        public string Status { get; set; } = "Active"; // Active, Inactive, Terminated
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } 
        public User? Admin { get; set; }
    }
}
