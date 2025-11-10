using AutoMapper;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.App.Users.Queries
{
    public class GetByEmail
    {
        public class Query(string email)
        {
            public string Email { get; set; } = email;
        }

        public class Handler(
            UserProfileDbContext context,
            IMapper mapper)
        {
            private readonly UserProfileDbContext _context = context;
            private readonly IMapper _mapper = mapper;

            public async Task<UserOutputDto?> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(
                    u => u.Email == request.Email,
                    CancellationToken.None);
                if (user == null)
                {
                    return null;
                }

                return _mapper.Map<UserOutputDto>(user);
            }
        }
    }
}
