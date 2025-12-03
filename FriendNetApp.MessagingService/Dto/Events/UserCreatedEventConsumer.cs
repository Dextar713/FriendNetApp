using FriendNetApp.MessagingService.Data;
using FriendNetApp.MessagingService.Models;
using MassTransit;
using FriendNetApp.Contracts.Events;

namespace FriendNetApp.MessagingService.Dto.Events
{
    public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly MessagingDbContext _db;

        public UserCreatedEventConsumer(MessagingDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;

            var exists = await _db.UserReplicas.FindAsync(message.Id);
            if (exists != null)
                return; // idempotency

            var replica = new UserReplica
            {
                Id = message.Id,
                Email = message.Email,
                UserName = message.UserName,
                ProfileImageUrl = message.ProfileImageUrl
            };

            _db.UserReplicas.Add(replica);
            await _db.SaveChangesAsync();
        }
    }

}
