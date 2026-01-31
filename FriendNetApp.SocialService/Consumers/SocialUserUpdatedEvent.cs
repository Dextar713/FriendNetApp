using FriendNetApp.Contracts.Events;
using FriendNetApp.SocialService.Data;
using MassTransit;

namespace FriendNetApp.SocialService.Consumers
{
    public class SocialUserUpdatedEventConsumer : IConsumer<SocialUserUpdatedEvent>
    {
        private readonly SocialDbContext _db;

        public SocialUserUpdatedEventConsumer(SocialDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<SocialUserUpdatedEvent> context)
        {
            var m = context.Message;

            var userNode = await _db.UserNodes.FindAsync(m.Id);
            if (userNode == null)
                return; // user not cached yet (orphan event)

            if (userNode.Age != m.Age)
                userNode.Age = m.Age;

            if (userNode.Description != m.Description)
                userNode.Description = m.Description;

            await _db.SaveChangesAsync(CancellationToken.None);
        }
    }
}
