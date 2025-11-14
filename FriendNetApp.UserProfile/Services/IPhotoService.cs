using FriendNetApp.UserProfile.CloudinaryPhotos;

namespace FriendNetApp.UserProfile.Services
{
    public interface IPhotoService
    {
        Task<PhotoUploadResult?> UploadPhoto(IFormFile file);
        Task<string> DeletePhoto(string publicId); 
        Task<(byte[] Data, string ContentType)> GetPhotoAsync(string publicId);
    }
}
