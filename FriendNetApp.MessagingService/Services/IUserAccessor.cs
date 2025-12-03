using FriendNetApp.MessagingService.Models;

namespace FriendNetApp.MessagingService.Services
{
    public interface IUserAccessor
    {
        Task<UserReplica> GetCurrentUserAsync();
    }
}
