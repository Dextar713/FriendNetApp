using FriendNetApp.Contracts.Events;
using FriendNetApp.SocialService.Data;
using MassTransit;

namespace FriendNetApp.SocialService.Consumers
{
    public class SocialUserDeletedEventConsumer: IConsumer<SocialUserDeletedEvent>
    {
        private readonly SocialDbContext _db;

        public SocialUserDeletedEventConsumer(SocialDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<SocialUserDeletedEvent> context)
        {
            var message = context.Message;

            var userNode = await _db.UserNodes.FindAsync(message.Id);
            if (userNode == null)
                return; // user already deleted

            _db.UserNodes.Remove(userNode);
            await _db.SaveChangesAsync();
        }
    }
}
