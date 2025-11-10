using AutoMapper;
using FriendNetApp.UserProfile.Data;
using FriendNetApp.UserProfile.Dto;
using FriendNetApp.UserProfile.Models;

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
            IMapper mapper)
        {
            private readonly UserProfileDbContext _context = context;
            private readonly IMapper _mapper = mapper;

            public async Task<string> Handle(Command command,
                CancellationToken cancellationToken)
            {
                var newUser = _mapper.Map<AppUser>(command.UserInput);
                await _context.Users.AddAsync(newUser, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return newUser.Id.ToString();
            }
        }
    }
}
