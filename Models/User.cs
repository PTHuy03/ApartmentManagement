using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ApartmentManagement.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Avatar { get; set; }
        public string Role { get; set; }
        public bool Status { get; set; }
    }
}
