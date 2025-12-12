using AutoMapper;
using FriendNetApp.SocialService.Data;
using FriendNetApp.SocialService.Dto;
using FriendNetApp.SocialService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.SocialService.App.Matching.Commands
{
    public class RandomMatch
    {
        public class Command
        {
            public required string UserId { get; set; }
            public required RandomMatchFiltersDto Filters { get; set; }
        }

        public class Handler(
            SocialDbContext context,
            IMapper mapper)
        {
            public async Task<MatchDto> Handle(Command command,
                CancellationToken cancellationToken)
            {
                var randomUserId = await context.UserNodes
                    .Where(u => u.Id.ToString() != command.UserId)
                    .OrderBy(r => Guid.NewGuid())
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                Match match = new Match
                {
                    Type = Models.MatchType.Random,
                    User1Id = Guid.Parse(command.UserId),
                    User2Id = randomUserId,
                };
                
                await context.Matches.AddAsync(match, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                return mapper.Map<MatchDto>(match);
            }
        }
    }
}
