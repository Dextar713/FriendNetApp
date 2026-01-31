using AutoMapper;
using FriendNetApp.MessagingService.Data;
using FriendNetApp.MessagingService.Dto;
using FriendNetApp.MessagingService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.MessagingService.App.Chats.Queries
{
    public class GetUserChats
    {
        public class Query
        {
            public required string UserId { get; set; }
        }

        public class Handler(
            MessagingDbContext context,
            IMapper mapper)
        {
            public async Task<ICollection<ChatDto>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var userChats = await context.Chats
                    .Where(ch => ch.User1Id.ToString() == request.UserId
                                 || ch.User2Id.ToString() == request.UserId)
                    .Include(ch => ch.Messages)
                    .Include(ch=> ch.User1)
                    .Include(ch=> ch.User2)
                    .ToListAsync(cancellationToken);
                return mapper.Map<ICollection<ChatDto>>(userChats);
            }
        }
    }
}
