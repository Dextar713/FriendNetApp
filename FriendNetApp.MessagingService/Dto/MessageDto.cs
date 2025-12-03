namespace FriendNetApp.MessagingService.Dto
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid SenderId { get; set; }
        public required string Content { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool IsRead { get; set; }
    }
}
