using AutoMapper;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using FriendNetApp.UserProfile.Models;
using FriendNetApp.UserProfile.Services;

namespace FriendNetApp.UserProfile.App.Profiles.Commands
{
    public class AddPhoto
    {
        public class Command
        {
            public required IFormFile File { get; set; }
        }
        public class Handler
        {
            private readonly UserProfileDbContext _context;
            private readonly IPhotoService _photoService;
            private readonly IUserAccessor _userAccessor;
            private readonly IMapper _mapper;
            public Handler(UserProfileDbContext context, IPhotoService photoService,
                IUserAccessor userAccessor, IMapper mapper)
            {
                _context = context;
                _photoService = photoService;
                _userAccessor = userAccessor;
                _mapper = mapper;
            }
            public async Task<PhotoDto> Handle(Command request, CancellationToken cancellationToken)
            {
                var uploadResult = await _photoService.UploadPhoto(request.File);
                if (uploadResult == null)
                {
                    throw new Exception("Problem uploading photo");
                }
                
                AppUser currentUser;
                try
                {
                    currentUser = await _userAccessor.GetCurrentUserAsync();
                } catch (Exception ex)
                {
                    throw new Exception("Could not retrieve current user", ex);
                }
                var photo = new Photo
                {
                    Url = uploadResult.Url,
                    PublicId = uploadResult.PublicId,
                    UserId = currentUser.Id
                };
                currentUser.ProfileImageUrl = photo.Url;
                currentUser.PhotoId = photo.Id;
                _context.Photos.Add(photo);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    throw new Exception("Problem saving photo to database");
                }
                return _mapper.Map<PhotoDto>(photo);
            }
        }
    }
}
