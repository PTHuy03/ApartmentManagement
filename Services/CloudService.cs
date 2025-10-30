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

        public async Task<List<string>> UploadRoomImgs(IFormFile[] files, string roomName)
        {
            var uploadedUrls = new List<string>();
            foreach(var file in files)
            {
                await using var stream = file.OpenReadStream();
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = $"ApartmentManagement/Room/{roomName}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    uploadedUrls.Add(uploadResult.SecureUrl.ToString());
                }
            }

            return uploadedUrls;
        }

        public async Task<bool> DeleteAvatar(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return false; 
            }

            try
            {
                var uri = new Uri(imageUrl);
                var publicIdWithExtension = Path.GetFileNameWithoutExtension(uri.LocalPath); // Ví dụ: ApartmentManagement/Avatar/abc_xyz
                var folderPath = "ApartmentManagement/Avatar/";
                var publicId = imageUrl.Contains(folderPath)
                    ? imageUrl.Substring(imageUrl.IndexOf(folderPath)).Replace(".jpg", "").Replace(".png", "")
                    : publicIdWithExtension;

                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);
                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteRoomImg(string imageUrl, string roomName)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return false; // Không xóa ảnh mặc định
            }

            try
            {
                var uri = new Uri(imageUrl);
                var publicIdWithExtension = Path.GetFileNameWithoutExtension(uri.LocalPath);
                var folderPath = $"ApartmentManagement/Room/{roomName}";
                var publicId = imageUrl.Contains(folderPath)
                    ? imageUrl.Substring(imageUrl.IndexOf(folderPath)).Replace(".jpg", "").Replace(".png", "")
                    : publicIdWithExtension;

                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);
                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }
    }
}
