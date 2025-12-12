using FriendNetApp.SocialService.Models;

namespace FriendNetApp.SocialService.Services
{
    public interface IUserAccessor
    {
        Task<UserNode> GetCurrentUserAsync();
    }
}