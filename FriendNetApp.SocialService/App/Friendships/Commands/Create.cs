using AutoMapper;
using FriendNetApp.SocialService.Data;
using FriendNetApp.SocialService.Dto;
using FriendNetApp.SocialService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.SocialService.App.Friendships.Commands
{
    public class Create
    {
        public class Command
        {
            public required Guid User1Id { get; set; }
            public required Guid User2Id { get; set; }
        }

        public class Handler(
            SocialDbContext context,
            IMapper mapper)
        {
            public async Task Handle(Command command,
                CancellationToken cancellationToken)
            {
                if (command.User1Id == command.User2Id)
                {
                    throw new ArgumentException("A user cannot be friends with themselves.");
                }
                if (! await context.UserNodes.AnyAsync(u => u.Id == command.User1Id, cancellationToken) ||
                    !await context.UserNodes.AnyAsync(u => u.Id == command.User2Id, cancellationToken))
                {
                    throw new ArgumentException("One or both users do not exist.");
                }
                Friendship friendship = new()
                {
                    User1Id = command.User1Id,
                    User2Id = command.User2Id,
                };

                await context.Friendships.AddAsync(friendship, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
