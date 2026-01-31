using AutoMapper;
using FriendNetApp.Contracts.Events;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FriendNetApp.UserProfile.App.Users.Commands
{
    public class Edit
    {
        public class Command(Guid userId, UserInputDto userInput)
        {
            public Guid UserId { get; set; } = userId;
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

            public async Task<UserOutputDto?> Handle(Command command,
                CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(
                    u => u.Id == command.UserId, cancellationToken);
                if (user == null)
                {
                    return null;
                }

                user = _mapper.Map(command.UserInput, user);
                await _context.SaveChangesAsync(cancellationToken);

                await _publish.Publish(new UserUpdatedEvent(
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.ProfileImageUrl
                ), cancellationToken);

                await _publish.Publish(new SocialUserUpdatedEvent(
                    user.Id,
                    user.Email,
                    user.Age,
                    user.Description
                ), cancellationToken);


                UserOutputDto userOutput = _mapper.Map<UserOutputDto>(user); 
                return userOutput;
            }
        }
    }
}