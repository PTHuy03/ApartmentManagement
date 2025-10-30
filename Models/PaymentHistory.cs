using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApartmentManagement.Models
{
    public class PaymentHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public string TenantId { get; set; }
        public User? User { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } //"Cash", "Bank Transfer", "Online Payment"
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string Note { get; set; }
        public string CreatedBy { get; set; }
        public User? Admin { get; set; }
    }
}
