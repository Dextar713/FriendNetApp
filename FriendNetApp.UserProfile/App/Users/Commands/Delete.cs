using AutoMapper;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using FriendNetApp.UserProfile.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.App.Users.Commands
{
    public class Delete
    {
        public class Command(Guid guid)
        {
            public Guid UserId { get; set; } = guid;
        }

        public class Handler(
            UserProfileDbContext context,
            IMapper mapper)
        {
            private readonly UserProfileDbContext _context = context;
            private readonly IMapper _mapper = mapper;

            public async Task<int> Handle(Command command,
                CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
                if (user == null)
                {
                    return 0;
                }

                _context.Users.Remove(user);
                var numRowsDeleted = await _context.SaveChangesAsync(cancellationToken);

                return numRowsDeleted;
            }
        }
    }
}