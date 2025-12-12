using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.SocialService.App.Friendships.Commands
{
    public class Delete
    {
        public class Command
        {
            public required Guid User1Id { get; set; }
            public required Guid User2Id { get; set; }
        }
        public class Handler(
            Data.SocialDbContext context)
        {
            public async Task Handle(Command command,
                CancellationToken cancellationToken)
            {
                var friendship = await context.Friendships
                    .FirstOrDefaultAsync(f =>
                        (f.User1Id == command.User1Id && f.User2Id == command.User2Id) ||
                        (f.User1Id == command.User2Id && f.User2Id == command.User1Id),
                        cancellationToken);
                if (friendship == null)
                {
                    throw new ArgumentException("Friendship does not exist.");
                }
                context.Friendships.Remove(friendship);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
