using FriendNetApp.Contracts.Events;
using FriendNetApp.MessagingService.Data;
using FriendNetApp.MessagingService.Models;
using MassTransit;

namespace FriendNetApp.MessagingService.Dto.Events
{
    public class UserDeletedEventConsumer: IConsumer<UserDeletedEvent>
    {
        private readonly MessagingDbContext _db;

        public UserDeletedEventConsumer(MessagingDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            var message = context.Message;

            var userReplica = await _db.UserReplicas.FindAsync(message.Id);
            if (userReplica == null)
                return; // user already deleted


            _db.UserReplicas.Remove(userReplica);
            await _db.SaveChangesAsync();
        }
    }
}
