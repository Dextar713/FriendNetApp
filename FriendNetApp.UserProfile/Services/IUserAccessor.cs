using FriendNetApp.UserProfile.Models;

namespace FriendNetApp.UserProfile.Services
{
    public interface IUserAccessor
    {
        Task<AppUser> GetCurrentUserAsync();
    }
}
