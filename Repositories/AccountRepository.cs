using ApartmentManagement.Models;
using ApartmentManagement.Services;
using MongoDB.Driver;
using ApartmentManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace ApartmentManagement.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly CloudService _cloudService;
        private readonly EmailSender _emailSender;
        private const string DefaultAvatarFileName = "https://res.cloudinary.com/dpr5nrste/image/upload/v1744902717/ApartmentManagement/Avatar/AvatarDefualt.png";

        public AccountRepository(IMongoDBService mongoDBService, CloudService cloudService, EmailSender emailSender)
        {
            _users = mongoDBService.GetCollection<User>("Users");
            _cloudService = cloudService;
            _emailSender = emailSender;
        }

        public async Task<List<User>> GetAllAccounts()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<User?> GetAccountById(string id)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            return user;
        }

        public async Task<(User UpdatedUser, bool EmailChanged)> UpdateAccount(string id, string fullName, string phoneNumber, string newEmail)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return (new User(), false);
            }

            bool emailChanged = user.Email != newEmail;

            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.Email = newEmail;

            var update = Builders<User>.Update
                .Set(u => u.FullName, user.FullName)
                .Set(u => u.PhoneNumber, user.PhoneNumber)
                .Set(u => u.Email, user.Email);

            await _users.UpdateOneAsync(u => u.Id == id, update);

            return (user, emailChanged);
        }

        public async Task<bool> ChangeRole(string id, string newRole)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return (false);

            var update = Builders<User>.Update.Set(u => u.Role, newRole);
            await _users.UpdateOneAsync(u => u.Id == id, update);

            return (true);
        }

        public async Task<bool> DeleteAccount(string id)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0)
                return (false);

            return (true);
        }

        public async Task<(bool Succsess, string Message)> Register(User user, string confirmPass)
        {
            var existingUser = await _users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (existingUser != null)
                return (false, "Email had existed!");

            if (user.PasswordHash != confirmPass)
                return (false, "Confirm password is incorrect");

            user.PasswordHash = HashPassword(user.PasswordHash);
            user.Role = "Tenant";
            user.Avatar = DefaultAvatarFileName;
            user.Status = false;

            await _users.InsertOneAsync(user);
            return (true, "Register Success");
        }

        public async Task<User?> Login(string email, string password)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null || user.PasswordHash != HashPassword(password))
                return null;

            return user;
        }

        public async Task<bool> ForgetPassword(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
                return false;

            string newPassword = GenerateRandomPassword(8);

            string hashedPassword = HashPassword(newPassword);

            user.PasswordHash = hashedPassword;
            user.Status = true;
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

            await _emailSender.SendEmailAsync(email, "Reset Password",
                $"Mật khẩu mới của bạn là: <strong>{newPassword}</strong>");

            return true;
        }

        public async Task<bool> ForceChangePassword(string id, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
                return false;

            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, HashPassword(newPassword))
                .Set(u => u.Status, false);
            await _users.UpdateOneAsync(u => u.Id == id, update);

            return true;
        }

        public async Task<User?> GetProfile(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> ChangePassword(string id, string oldPassword, string newPassword, string confirmPass)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return false;

            if (user.PasswordHash != HashPassword(oldPassword))
                return false;

            if (newPassword != confirmPass)
                return false;

            var update = Builders<User>.Update.Set(u => u.PasswordHash, HashPassword(newPassword));
            await _users.UpdateOneAsync(u => u.Id == id, update);

            return true;
        }

        public async Task<string> UploadAvatar(string id, IFormFile avatarFile)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return "";

            string imageUrl = await _cloudService.UploadAvatar(avatarFile);

            var update = Builders<User>.Update.Set(u => u.Avatar, imageUrl);
            await _users.UpdateOneAsync(u => u.Id == id, update);

            return imageUrl;
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
