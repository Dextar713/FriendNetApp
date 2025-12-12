using AutoMapper;
using FriendNetApp.MessagingService.Data;
using FriendNetApp.MessagingService.Dto;
using FriendNetApp.MessagingService.Exceptions;
using FriendNetApp.MessagingService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.MessagingService.App.Chats.Commands
{
    public class Delete
    {
        public class Command
        {
            public required string ChatId { get; set; }
        }

        public class Handler(
            MessagingDbContext context,
            IMapper mapper)
        {
            public async Task<string?> Handle(Command command,
                CancellationToken cancellationToken)
            {
                
                var chat = await context.Chats.FirstOrDefaultAsync(
                    u => u.Id.ToString() == command.ChatId,
                    cancellationToken);
                if (chat == null)
                {
                    throw new NotFoundException("Chat not found");
                }

                context.Chats.Remove(chat);
                bool res = await context.SaveChangesAsync(cancellationToken) > 0;
                return res ? chat.Id.ToString() : null;
            }
        }
    }
}