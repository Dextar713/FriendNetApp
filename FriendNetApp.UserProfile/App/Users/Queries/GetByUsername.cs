using AutoMapper;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.App.Users.Queries
{
    public class GetByUsername
    {
        public class Query(string userName)
        {
            public string UserName { get; set; } = userName;
        }

        public class Handler(
            UserProfileDbContext context,
            IMapper mapper)
        {
            private readonly UserProfileDbContext _context = context;
            private readonly IMapper _mapper = mapper;

            public async Task<ICollection<UserOutputDto>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var users = await _context.Users.Where(
                    u => u.UserName == request.UserName)
                    .ToListAsync(CancellationToken.None);
                return _mapper.Map<ICollection<UserOutputDto>>(users);
            }
        }
    }
}