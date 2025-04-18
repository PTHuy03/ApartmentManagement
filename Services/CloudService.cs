using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ApartmentManagement.Services
{
    public class CloudService
    {
        private readonly Cloudinary _cloudinary;

        public CloudService(IConfiguration configuration)
        {
            var cloudSeccion = configuration.GetSection("CloudinarySettings");
            var acc = new Account(
                    cloudSeccion["CloudName"], cloudSeccion["ApiKey"], cloudSeccion["ApiSecret"]
                );
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> UploadAvatar(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = "ApartmentManagement/Avatar"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
    }
}
