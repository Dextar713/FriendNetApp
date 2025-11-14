using AutoMapper;
using FriendNetApp.UserProfile.App.Users.Queries;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.App.Profiles.Queries
{
    public class GetProfilePhoto
    {
        public class Query
        {
            public required Guid UserId { get; set; }
        }

        public class Handler(UserProfileDbContext context, IMapper mapper)
        {
            public async Task<PhotoDto?> Handle(Query request, CancellationToken cancellationToken)
            {
                var photo = await context.Users
                    .Where(u => u.Id == request.UserId)
                    .Select(u => u.Photo)
                    .FirstOrDefaultAsync(cancellationToken);

                if (photo == null)
                {
                    return null;
                }

                return mapper.Map<PhotoDto>(photo);
            }
        }
    }
}
