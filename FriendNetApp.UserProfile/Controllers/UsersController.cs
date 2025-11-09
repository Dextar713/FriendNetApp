using FriendNetApp.UserProfile.App.Users.Queries;
using FriendNetApp.UserProfile.Dto;
using FriendNetApp.UserProfile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FriendNetApp.UserProfile.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController(ILogger<UsersController> logger,
        GetAll.Handler getAll) : ControllerBase
    {

        private readonly ILogger<UsersController> _logger = logger;
        private readonly GetAll.Handler _getAll = getAll;

        [Authorize(Roles="Client,Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _getAll.Handle(new GetAll.Query(), CancellationToken.None);
            return Ok(users);
        }
    }
}
