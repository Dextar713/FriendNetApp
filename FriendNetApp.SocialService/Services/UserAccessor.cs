using System.Security.Claims;
using FriendNetApp.SocialService.Data;
using FriendNetApp.SocialService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.SocialService.Services
{
    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SocialDbContext _context;

        public UserAccessor(IHttpContextAccessor httpContextAccessor, SocialDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<UserNode> GetCurrentUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("No active HttpContext.");

            var user = httpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            // Try standard email claim types
            var email = user.FindFirst(ClaimTypes.Email)?.Value
                        ?? user.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(email))
                throw new InvalidOperationException("Email claim not found in token.");

            var appUser = await _context.UserNodes.FirstOrDefaultAsync(u => u.Email == email, CancellationToken.None);
            if (appUser == null)
                throw new KeyNotFoundException($"User with email '{email}' not found in UserProfile database.");

            return appUser;
        }

    }
}
