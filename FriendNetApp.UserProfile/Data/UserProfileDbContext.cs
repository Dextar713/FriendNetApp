using FriendNetApp.UserProfile.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.Data
{
    public class UserProfileDbContext(DbContextOptions<UserProfileDbContext> options): DbContext(options)
    {
        public DbSet<AppUser> Users { get; set; }
    }
}
