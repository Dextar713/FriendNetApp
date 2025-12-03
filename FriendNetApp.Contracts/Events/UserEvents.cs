namespace FriendNetApp.Contracts.Events
{
    public record UserCreatedEvent
    (
        Guid Id,
        string UserName,
        string Email,
        string ProfileImageUrl
    );

    public record UserUpdatedEvent
    (
        Guid Id,
        string UserName,
        string Email,
        string ProfileImageUrl
    );
}