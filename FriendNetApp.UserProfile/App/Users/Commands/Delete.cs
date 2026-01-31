using AutoMapper;
using FriendNetApp.Contracts.Events;
using FriendNetApp.UserProfile.Data;
using MassTransit;
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
            IMapper mapper,
            IPublishEndpoint publish)
        {
            private readonly UserProfileDbContext _context = context;
            private readonly IMapper _mapper = mapper;
            private readonly IPublishEndpoint _publish = publish;

            public async Task<int> Handle(Command command,
                CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
                if (user == null)
                {
                    return 0;
                }

                var userId = user.Id;

                _context.Users.Remove(user);
                var numRowsDeleted = await _context.SaveChangesAsync(cancellationToken);

                await _publish.Publish(new UserDeletedEvent(
                    userId), cancellationToken);

                await _publish.Publish(new SocialUserDeletedEvent(
                    userId), cancellationToken);

                return numRowsDeleted;
            }
        }
    }
}