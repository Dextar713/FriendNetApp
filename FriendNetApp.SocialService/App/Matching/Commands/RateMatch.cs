using AutoMapper;
using FriendNetApp.SocialService.Data;
using FriendNetApp.SocialService.Dto;
using FriendNetApp.SocialService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.SocialService.App.Matching.Commands
{
    public class RateMatch
    {
        public class Command
        {
            public required Guid UserId { get; set; }
            public required Guid MatchId { get; set; }
            public required int Rating { get; set; }
        }

        public class Handler(
            SocialDbContext context,
            IMapper mapper)
        {
            public async Task<bool> Handle(Command command,
                CancellationToken cancellationToken)
            {
                Match? match = await context.Matches.FirstOrDefaultAsync(
                    m => m.Id == command.MatchId, cancellationToken);
                if (match == null)
                {
                    return false;
                }
                if (match.User1Id == command.UserId)
                {
                    match.Rating1 = command.Rating;
                }
                else if (match.User2Id == command.UserId)
                {
                    match.Rating2 = command.Rating;
                }
                else
                {
                    throw new InvalidOperationException("User is not in this match");
                }
                await context.SaveChangesAsync(cancellationToken);
                return true;
            }
        }
    }
}
