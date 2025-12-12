using AutoMapper;
using FriendNetApp.SocialService.Data;
using FriendNetApp.SocialService.Dto;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.SocialService.App.Friendships.Queries
{
    public class GetAll
    {
        public class Query
        {
            public required Guid UserId { get; set; }
        }
        public class Handler(
            SocialDbContext context,
            IMapper mapper)
        {
            public async Task<List<FriendshipDto>> Handle(Query query,
                CancellationToken cancellationToken)
            {
                var friendships = await context.Friendships
                    .Where(m => m.User1Id == query.UserId || m.User2Id == query.UserId)
                    .Include(m => m.User1)
                    .Include(m => m.User2)
                    .ToListAsync(cancellationToken);
                return mapper.Map<List<FriendshipDto>>(friendships);
            }
        }
    }
}
