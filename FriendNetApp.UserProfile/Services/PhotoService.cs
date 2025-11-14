using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FriendNetApp.UserProfile.CloudinaryPhotos;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace FriendNetApp.UserProfile.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }
        public async Task<string> DeletePhoto(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Error == null ? result.Result : throw new Exception(result.Error.Message);
        }

        public async Task<PhotoUploadResult?> UploadPhoto(IFormFile file)
        {
            if (file.Length > 0)
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                    Folder = "friendnetapp/userprofiles"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                {
                    throw new Exception(uploadResult.Error.Message);
                }
                return new PhotoUploadResult
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.SecureUrl.AbsoluteUri
                };
            }

            return null;
        }

        public async Task<(byte[] Data, string ContentType)> GetPhotoAsync(string publicId)
        {
            try
            {
                var resource = _cloudinary.GetResource(new GetResourceParams(publicId));
                var secureUrl = resource?.SecureUrl ?? resource?.Url;
                if (secureUrl == null)
                    throw new Exception("Cloudinary resource has no URL");

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(secureUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Unable to get photo from Cloudinary URL. Status: {response.StatusCode}");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                var data = await response.Content.ReadAsByteArrayAsync();
                return (data, contentType);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve Cloudinary resource", ex);
            }
        }

    }
}
