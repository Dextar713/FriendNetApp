using FriendNetApp.UserProfile.App.Profiles.Commands;
using FriendNetApp.UserProfile.App.Profiles.Queries;
using FriendNetApp.UserProfile.App.Users.Queries;
using FriendNetApp.UserProfile.Dto;
using FriendNetApp.UserProfile.Models;
using FriendNetApp.UserProfile.Services;
using FriendNetApp.UserProfile.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FriendNetApp.UserProfile.Controllers
{
    [ApiController]
    [Route("users/profiles")]
    public class ProfilesController(ILogger<ProfilesController> logger,
        AddPhoto.Handler addPhoto,
        GetProfilePhoto.Handler getPhoto,
        DeletePhoto.Handler deletePhoto,
        IPhotoService photoService) : ControllerBase
    {
        
        private readonly ILogger<ProfilesController> _logger = logger;
        private readonly AddPhoto.Handler _addPhoto = addPhoto;
        private readonly GetProfilePhoto.Handler _getPhoto = getPhoto;
        private readonly DeletePhoto.Handler _deletePhoto = deletePhoto;
        private readonly IPhotoService _photoService = photoService;

        [Authorize(Roles = "Client,Admin")]
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto([FromForm] IFormFile file)
        {
            try
            {
                var photo = await _addPhoto.Handle(new AddPhoto.Command {File = file}, 
                    CancellationToken.None);
                return Ok(photo);
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Error adding photo");
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet("{userId}/photo")]
        public async Task<ActionResult<PhotoDto>> GetPhotoForUser(string userId)
        {
            try
            {
                Guid guid = Guid.Parse(userId);
                var photo = await _getPhoto.Handle(new GetProfilePhoto.Query
                {
                    UserId = guid
                }, CancellationToken.None);
                if (photo == null)
                {
                    return Ok("No profile photo");
                }

                return Ok(photo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photo");
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpDelete("photo")]
        public async Task<ActionResult<PhotoDto>> DeletePhotoForUser()
        {
            try
            {
                bool result = await _deletePhoto.Handle(new DeletePhoto.Command { }, 
                    CancellationToken.None);
                if (!result)
                {
                    return NotFound("No profile photo to delete");
                }

                return Ok("Photo successfully deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo");
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Client,Admin")]
        [HttpGet("photo/{photoId}")]
        public async Task<IActionResult> GetPhotoById(string photoId)
        {
            try
            {
                Guid id = Guid.Parse(photoId);
                // Find photo record in DB
                using var scope = HttpContext.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<UserProfileDbContext>();
                var photo = await db.Photos.FindAsync(id);
                if (photo == null)
                {
                    return NotFound();
                }

                var result = await _photoService.GetPhotoAsync(photo.PublicId);
                byte[] data = result.Data;
                string contentType = result.ContentType;
                return File(data, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photo by id");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
