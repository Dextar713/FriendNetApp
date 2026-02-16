using FriendNetApp.MessagingService.App.Chats.Commands;
using FriendNetApp.MessagingService.Models;
using FriendNetApp.MessagingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FriendNetApp.MessagingService.Hubs
{
    [Authorize(Roles = "Admin,Client")]
    public class ChatHub(SendMessage.Handler sendHandler,
        IUserAccessor userAccessor) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is null");
            }
            var chatId = httpContext.Request.Query["chatId"].ToString();
            if (string.IsNullOrEmpty(chatId))
            {
                throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            //await Clients.Caller.SendAsync("LoadMessages", null);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is null");
            }
            var chatId = httpContext.Request.Query["chatId"].ToString();
            if (string.IsNullOrEmpty(chatId))
            {
                throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task SendMessage(SendMessage.Command command)
        {
            var curUser = await userAccessor.GetCurrentUserAsync();
            if (curUser.Id != command.Message.SenderId)
            {
                throw new HubException(curUser.Email + "not authenticated");
            }
            if (command?.Message == null) throw new ArgumentNullException(nameof(command));
            var msgId = await sendHandler.Handle(command, CancellationToken.None);
            await Clients.Group(command.Message.ChatId.ToString())
                .SendAsync("ReceiveMessage", command.Message);
        }

        //public async Task JoinChatGroup(string chatId)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        //}

        //public async Task LeaveChatGroup(string chatId)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        //}
    }
}
