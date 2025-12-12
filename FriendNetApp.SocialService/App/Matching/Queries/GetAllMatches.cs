using AutoMapper;
using FriendNetApp.SocialService.Data;
using FriendNetApp.SocialService.Dto;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.SocialService.App.Matching.Queries
{
    public class GetAllMatches
    {
        public class Query
        {
            public required Guid UserId { get; set; }
        }
        public class Handler(
            SocialDbContext context,
            IMapper mapper)
        {
            public async Task<List<MatchDto>> Handle(Query query,
                CancellationToken cancellationToken)
            {
                var matches = await context.Matches
                    .Where(m => m.User1Id == query.UserId || m.User2Id == query.UserId)
                    .Include(m => m.User1)
                    .Include(m => m.User2)
                    .Include(m => m.Inviter)
                    .ToListAsync(cancellationToken);
                return mapper.Map<List<MatchDto>>(matches);
            }
        }
    }
}
