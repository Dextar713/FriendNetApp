using System.Security.Authentication;
using AutoMapper;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using FriendNetApp.UserProfile.Models;
using FriendNetApp.UserProfile.Services;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.App.Profiles.Commands
{
    public class DeletePhoto
    {
        public class Command { }
        public class Handler
        {
            private readonly UserProfileDbContext _context;
            private readonly IPhotoService _photoService;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;
            public Handler(UserProfileDbContext context, IPhotoService photoService,
                IMapper mapper, IUserAccessor userAccessor)
            {
                _context = context;
                _photoService = photoService;
                _mapper = mapper;
                _userAccessor = userAccessor;
            }
            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var currentUser = await _userAccessor.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    throw new AuthenticationException("User not authenticated");
                }

                var photo = await _context.Photos.FirstOrDefaultAsync(
                    ph => ph.UserId == currentUser.Id, cancellationToken);
                if (photo == null)
                {
                    return false;
                }

                await _photoService.DeletePhoto(photo.PublicId);
                _context.Remove(photo);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                return result;
            }
        }
    }
}
