using AutoMapper;
using FriendNetApp.MessagingService.Data;
using FriendNetApp.MessagingService.Dto;
using FriendNetApp.MessagingService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.MessagingService.App.Chats.Queries
{
    public class GetChatHistory
    {
        public class Query
        {
            public required Guid ChatId { get; set; }
        }

        public class Handler(
            MessagingDbContext context,
            IMapper mapper)
        {
            public async Task<ICollection<MessageDto>?> Handle(Query request,
                CancellationToken cancellationToken)
            {
                Chat? chat = await context.Chats
                    .Include(ch => ch.Messages)
                    .FirstOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);
                if (chat == null)
                {
                    return null;
                }

                var messages = chat.Messages.TakeLast(10).ToList();
                return mapper.Map<ICollection<MessageDto>>(messages);
            }
        }
    }
}
