using FriendNetApp.SocialService.App.Friendships.Commands;
using FriendNetApp.SocialService.App.Friendships.Queries;
using FriendNetApp.SocialService.Dto;
using FriendNetApp.SocialService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FriendNetApp.SocialService.Controllers
{
    [Route("social/friendships")]
    public class FriendshipsController(ILogger<FriendshipsController> logger,
        IUserAccessor userAccessor,
        GetAll.Handler getAll,
        Create.Handler create,
        Delete.Handler delete) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> GetFriendships()
        {
            try
            {
                var user = await userAccessor.GetCurrentUserAsync();
                var friendships = await getAll.Handle(new GetAll.Query
                {
                    UserId = user.Id
                }, CancellationToken.None);
                return Ok(friendships);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving friendships");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> CreateFriendship([FromBody] CreateFriendshipRequestDto request)
        {
            try
            {
                await create.Handle(new Create.Command
                {
                    User1Id = request.User1Id,
                    User2Id = request.User2Id
                }, CancellationToken.None);
                return Ok("Friendship created");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating friendship");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,Client")]
        public async Task<IActionResult> DeleteFriendship([FromBody] CreateFriendshipRequestDto request)
        {
            var user = await userAccessor.GetCurrentUserAsync();
            if (user.Id != request.User1Id && user.Id != request.User2Id)
            {
                return Forbid("You can only delete friendships that you are part of.");
            }
            try
            {
                await delete.Handle(new Delete.Command
                {
                    User1Id = request.User1Id,
                    User2Id = request.User2Id
                }, CancellationToken.None);
                return Ok("Friendship deleted");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating friendship");
                return BadRequest(ex.Message);
            }
        }
    }
}
