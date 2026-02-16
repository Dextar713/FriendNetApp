using AutoMapper;
using FriendNetApp.MessagingService.Data;
using FriendNetApp.MessagingService.Exceptions;
using FriendNetApp.MessagingService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.MessagingService.App.Chats.Commands
{
    public class Create
    {
        public class Command
        {
            public required string User1Id { get; set; }
            public required string User2Id { get; set; }
        }

        public class Handler(
            MessagingDbContext context,
            IMapper mapper)
        {
            public async Task<string?> Handle(Command command,
                CancellationToken cancellationToken)
            {
                var user1 = await context.UserReplicas.FirstOrDefaultAsync(
                    u => u.Id.ToString() == command.User1Id,
                    cancellationToken);
                if (user1 == null)
                {
                    throw new NotFoundException("User 1 not found " + command.User1Id.Length);
                }
                var user2 = await context.UserReplicas.FirstOrDefaultAsync(
                    u => u.Id.ToString() == command.User2Id,
                    cancellationToken);
                if (user2 == null)
                {
                    throw new NotFoundException("User 2 not found");
                }

                bool chatExists = await context.Chats.AnyAsync(c =>
                        (c.User1Id.ToString() == command.User1Id && c.User2Id.ToString() == command.User2Id) ||
                        (c.User1Id.ToString() == command.User2Id && c.User2Id.ToString() == command.User1Id),
                    cancellationToken);
                if (chatExists)
                {
                    throw new Exception("Chat between these users already exists");
                }

                Chat chat = new Chat
                {
                    User1Id = user1.Id,
                    User2Id = user2.Id,
                    //StartedAt = DateTime.UtcNow
                };
                await context.Chats.AddAsync(chat, cancellationToken);
                bool res = await context.SaveChangesAsync(cancellationToken) > 0;
                return res ? chat.Id.ToString() : null;
            }
        }
    }
}
