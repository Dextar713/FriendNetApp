using FriendNetApp.UserProfile.App.Users.Commands;
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
        GetAll.Handler getAll,
        GetById.Handler getById,
        GetByEmail.Handler getByEmail,
        GetByUsername.Handler getByUsername,
        Create.Handler create,
        Delete.Handler delete,
        Edit.Handler edit) : ControllerBase
    {

        private readonly ILogger<UsersController> _logger = logger;
        private readonly GetAll.Handler _getAll = getAll;
        private readonly GetById.Handler _getById = getById;
        private readonly GetByEmail.Handler _getByEmail = getByEmail;
        private readonly GetByUsername.Handler _getByUsername = getByUsername;
        private readonly Create.Handler _create = create;
        private readonly Delete.Handler _delete = delete;
        private readonly Edit.Handler _edit = edit;

        [Authorize(Roles="Client,Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _getAll.Handle(new GetAll.Query(), CancellationToken.None);
            return Ok(users);
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            Guid guid = Guid.Parse(id);
            var user = await _getById.Handle(new GetById.Query(guid), CancellationToken.None);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet("find-by-email")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            var user = await _getByEmail.Handle(new GetByEmail.Query(email), CancellationToken.None);
            return Ok(user);
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet("find-by-username")]
        public async Task<IActionResult> GetByUserName([FromQuery] string userName)
        {
            var users = await _getByUsername.Handle(new GetByUsername.Query(userName), CancellationToken.None);
            return Ok(users);
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] UserInputDto userInput)
        {
            try
            {
                string newUserId = await _create.Handle(new Create.Command(userInput), CancellationToken.None);
                return Ok(newUserId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpPatch("edit/{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] UserInputDto userInput)
        {
            try
            {
                Guid guid = Guid.Parse(id);
                UserOutputDto? userOutput = await _edit.Handle(new Edit.Command(guid, userInput), CancellationToken.None);
                if (userOutput == null)
                {
                    return NotFound("User not found");
                }

                return Ok(userOutput);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            Guid guid = Guid.Parse(id);
            try
            {
                int numRowsDeleted = await _delete.Handle(new Delete.Command(guid), CancellationToken.None);
                if (numRowsDeleted ==0)
                {
                    return NotFound("User not found with this id");
                }

                return Ok("User successfully deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
