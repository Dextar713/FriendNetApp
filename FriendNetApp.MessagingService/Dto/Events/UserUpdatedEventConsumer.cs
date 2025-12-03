using FriendNetApp.Contracts.Events;
using FriendNetApp.MessagingService.Data;
using MassTransit;

namespace FriendNetApp.MessagingService.Dto.Events
{
    public class UserUpdatedEventConsumer : IConsumer<UserUpdatedEvent>
    {
        private readonly MessagingDbContext _db;

        public UserUpdatedEventConsumer(MessagingDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
        {
            var m = context.Message;

            var replica = await _db.UserReplicas.FindAsync(m.Id);
            if (replica == null)
                return; // user not cached yet (orphan event)

            if (m.UserName != null)
                replica.UserName = m.UserName;

            if (m.ProfileImageUrl != null)
                replica.ProfileImageUrl = m.ProfileImageUrl;

            await _db.SaveChangesAsync();
        }
    }

}
