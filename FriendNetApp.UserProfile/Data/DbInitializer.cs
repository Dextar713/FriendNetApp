using FriendNetApp.UserProfile.Models;

namespace FriendNetApp.UserProfile.Data
{
    public class DbInitializer
    {
        public static async Task SeedData(UserProfileDbContext context)
        {
            if (!context.Users.Any())
            {
                var users = new List<AppUser>
                {
                    new AppUser
                    {
                        Email = "dex@gmail.com",
                        UserName = "Dextar",
                        Age = 20
                    },
                    new AppUser
                    {
                        Email = "mihail@yahoo.com",
                        UserName = "Mihail",
                        Age = 21
                    },
                    new AppUser
                    {
                        Email = "eusebiu@gmail.com",
                        UserName = "Eusebiu",
                        Age = 21
                    }
                };
                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }
    }
}
