using FriendNetApp.SocialService.App.Matching.Commands;
using FriendNetApp.SocialService.App.Matching.Queries;
using FriendNetApp.SocialService.Dto;
using FriendNetApp.SocialService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FriendNetApp.SocialService.Controllers
{
    [Route("social/matching")]
    public class MatchingController(ILogger<MatchingController> logger,
        IUserAccessor userAccessor,
        RandomMatch.Handler randomHandler,
        FriendMatch.Handler friendHandler,
        GetAllMatches.Handler getAllHandler,
        RateMatch.Handler rateHandler) : ControllerBase
    {
        [HttpPost("random")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> MatchRandom([FromBody] RandomMatchFiltersDto filters)
        {
            var curUser = await userAccessor.GetCurrentUserAsync();
            var userId = curUser.Id.ToString();
            try
            {
                var result = await randomHandler.Handle(
                    new RandomMatch.Command { UserId = userId, Filters = filters },
                    CancellationToken.None);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("from-friend")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> MatchFriends([FromBody] FriendMatchRequestDto request)
        {
            var inviter = await userAccessor.GetCurrentUserAsync();
            request.InviterId = inviter.Id;
            try
            {
                MatchDto match = await friendHandler.Handle(
                    new FriendMatch.Command
                    {
                        Request = request
                    },
                    CancellationToken.None);

                return Ok(match);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }

        // 3. Rate a Match (POST)
        [HttpPost("{matchId}/rate")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> Rate(string matchId, [FromBody] int rating)
        {
            var user = await userAccessor.GetCurrentUserAsync();
            try
            {
                var success = await rateHandler.Handle(new RateMatch.Command
                {
                    UserId = user.Id,
                    MatchId = Guid.Parse(matchId),
                    Rating = rating
                }, CancellationToken.None);
                if (success)
                {
                    return Ok("Match rated");
                }
                return NotFound("Match not found");
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles="Admin,Client")]
        public async Task<IActionResult> GetAllMatches()
        {
            try
            {
                var curUser = await userAccessor.GetCurrentUserAsync();
                var matches = await getAllHandler.Handle(new GetAllMatches.Query
                {
                    UserId = curUser.Id
                }, CancellationToken.None);
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }
    }
}
