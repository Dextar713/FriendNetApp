using AutoMapper;
using FriendNetApp.SocialService.Data;
using FriendNetApp.SocialService.Dto;
using FriendNetApp.SocialService.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.SocialService.App.Matching.Commands
{
    public class FriendMatch
    {
        public class Command
        {
            public required FriendMatchRequestDto Request { get; set; }
        }

        public class Handler(
            SocialDbContext context,
            IMapper mapper)
        {
            public async Task<MatchDto> Handle(Command command,
                CancellationToken cancellationToken)
            {
                var req = command.Request;
                if (!req.InviterId.HasValue)
                {
                    throw new ArgumentException("InviterId must be provided for friend-invited matches");
                }
                if (req.UserAId == req.UserBId || req.InviterId == req.UserBId
                    || req.UserBId == req.InviterId)
                {
                    throw new ArgumentException("UserAId, UserBId, InviterId should be different");
                }

                var inviterId = req.InviterId.Value;

                var inviterFriendWithA = await context.Friendships.AnyAsync(f =>
                    (f.User1Id == inviterId && f.User2Id == req.UserAId) ||
                    (f.User1Id == req.UserAId && f.User2Id == inviterId), cancellationToken);

                var inviterFriendWithB = await context.Friendships.AnyAsync(f =>
                    (f.User1Id == inviterId && f.User2Id == req.UserBId) ||
                    (f.User1Id == req.UserBId && f.User2Id == inviterId), cancellationToken);

                if (!inviterFriendWithA || !inviterFriendWithB)
                {
                    throw new InvalidOperationException("Inviter must be friends with both UserA and UserB to create this match.");
                }

                var matchExists = await context.Matches.AnyAsync(m =>
                    (m.User1Id == req.UserAId && m.User2Id == req.UserBId) ||
                    (m.User1Id == req.UserBId && m.User2Id == req.UserAId),
                    cancellationToken);
                if (matchExists)
                {
                    throw new Exception("Match already exists between users");
                }

                Match match = new Match
                {
                    Type = Models.MatchType.FromFriend,
                    User1Id = req.UserAId,
                    User2Id = req.UserBId,
                    InviterId = req.InviterId,
                };

                await context.Matches.AddAsync(match, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return mapper.Map<MatchDto>(match);
            }
        }
    }
}
