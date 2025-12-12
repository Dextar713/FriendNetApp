using FriendNetApp.Contracts.Events;
using FriendNetApp.SocialService.Data;
using FriendNetApp.SocialService.Models;
using MassTransit;

namespace FriendNetApp.SocialService.Consumers
{
    public class SocialUserCreatedEventConsumer : IConsumer<SocialUserCreatedEvent>
    {
        private readonly SocialDbContext _db;

        public SocialUserCreatedEventConsumer(SocialDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<SocialUserCreatedEvent> context)
        {
            var message = context.Message;

            var exists = await _db.UserNodes.FindAsync(message.Id);
            if (exists != null)
                return; // idempotency

            var userNode = new UserNode
            {
                Id = message.Id,
                Email = message.Email,
                Age = message.Age
            };

            _db.UserNodes.Add(userNode);
            await _db.SaveChangesAsync();
        }
    }
}
