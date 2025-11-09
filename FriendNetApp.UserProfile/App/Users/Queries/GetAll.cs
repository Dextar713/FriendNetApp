using AutoMapper;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using FriendNetApp.UserProfile.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.App.Users.Queries
{
    public class GetAll
    {
        public class Query {}

        public class Handler(UserProfileDbContext context, IMapper mapper)
        {
            private readonly UserProfileDbContext _context = context;
            private readonly IMapper _mapper = mapper;

            public async Task<ICollection<UserOutputDto>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var users = await _context.Users.ToListAsync(cancellationToken);
                return _mapper.Map<ICollection<UserOutputDto>>(users);
            }
        }
    }
}
