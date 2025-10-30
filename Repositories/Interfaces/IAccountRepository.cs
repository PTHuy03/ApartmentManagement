using ApartmentManagement.Models;
using Microsoft.AspNetCore.Http;

namespace ApartmentManagement.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<List<User>> GetAllAccounts();
        Task<User?> GetAccountById(string id);
        Task<(User UpdatedUser, bool EmailChanged)> UpdateAccount(string id, string fullName, string phoneNumber, string newEmail);
        Task<bool> ChangeRole(string id, string newRole);
        Task<bool> DeleteAccount(string id);
        Task<(bool Succsess, string Message)> Register(User user, string confirmPass);
        Task<User?> Login(string email, string password);
        Task<bool> ForgetPassword(string email);
        Task<bool> ForceChangePassword(string id, string newPassword, string confirmPassword);
        Task<User?> GetProfile(string id);
        Task<bool> ChangePassword(string id, string oldPassword, string newPassword, string confirmPass);
        Task<string> UploadAvatar(string id, IFormFile avatarFile);
    }
}
