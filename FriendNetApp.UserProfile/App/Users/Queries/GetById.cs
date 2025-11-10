using AutoMapper;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.App.Users.Queries
{
    public class GetById
    {
        public class Query(Guid guid)
        {
            public Guid Id { get; set; } = guid;
        }

        public class Handler(UserProfileDbContext context,
            IMapper mapper)
        {
            private readonly UserProfileDbContext _context = context;
            private readonly IMapper _mapper = mapper;

            public async Task<UserOutputDto?> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(
                    u => u.Id == request.Id, CancellationToken.None);
                if (user == null)
                {
                    return null;
                }

                return _mapper.Map<UserOutputDto>(user);
            }
        }
    }
}
