using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendNetApp.Contracts.Events
{
    public record SocialUserCreatedEvent
    (
        Guid Id,
        string Email,
        int? Age,
        string Description
    );

    public record SocialUserUpdatedEvent
    (
        Guid Id,
        string Email, 
        int? Age,
        string Description
    );

    public record SocialUserDeletedEvent
    (
        Guid Id
    );
}
