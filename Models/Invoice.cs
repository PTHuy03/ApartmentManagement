using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ApartmentManagement.Models
{
    public class Invoice
    {
        [BsonId]
        [BsonRepresentation (BsonType.ObjectId)]
        public string Id { get; set; }
        public string ContractId { get; set; }
        public Contract? Contract { get; set; }
        public string RoomId { get; set; }
        public Room? Room { get; set; }
        public string TenantId { get; set; }
        public User? User { get; set; }
        public int Month { get; set; } // 1-12
        public int Year { get; set; } // YYYY
        public decimal RentAmount { get; set; }
        public decimal ElectricAmount { get; set; }
        public decimal WaterAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public User? Admin { get; set; }
        public DateTime? paidAt { get; set; }
    } 
}
