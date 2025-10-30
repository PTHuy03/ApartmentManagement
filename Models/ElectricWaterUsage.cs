using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ApartmentManagement.Models
{
    public class ElectricWaterUsage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string RoomId { get; set; }
        public Room? Room { get; set; }
        public int Month { get; set; } // 1-12
        public int Year { get; set; } // YYYY
        public int ElectricStart { get; set; }
        public int ElectricEnd { get; set; }
        public int? WaterStart { get; set; }
        public int? WaterEnd { get; set; }
        public decimal UnitPriceElectric { get; set; } // Price per kWh
        public decimal? UnitPriceWater { get; set; } // Price per m3
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } 
        public User? Admin { get; set; }
    }
}
