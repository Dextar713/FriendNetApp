using AutoMapper;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using FriendNetApp.UserProfile.Models;
using MassTransit;
using FriendNetApp.Contracts.Events;

namespace FriendNetApp.UserProfile.App.Users.Commands
{
    public class Create
    {
        public class Command(UserInputDto userInput)
        {
            public UserInputDto UserInput { get; set; } = userInput;
        }

        public class Handler(
            UserProfileDbContext context,
            IMapper mapper,
            IPublishEndpoint publish)
        {
            private readonly UserProfileDbContext _context = context;
            private readonly IMapper _mapper = mapper;
            private readonly IPublishEndpoint _publish = publish;

            public async Task<string> Handle(Command command,
                CancellationToken cancellationToken)
            {
                var newUser = _mapper.Map<AppUser>(command.UserInput);
                await _context.Users.AddAsync(newUser, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await _publish.Publish(new UserCreatedEvent(
                    newUser.Id,
                    newUser.UserName,
                    newUser.Email,
                    newUser.ProfileImageUrl
                ), cancellationToken);
                await _publish.Publish(new SocialUserCreatedEvent(
                    newUser.Id,
                    newUser.Email,
                    newUser.Age,
                    newUser.Description
                ), cancellationToken);
                return newUser.Id.ToString();
            }
        }
    }
}
