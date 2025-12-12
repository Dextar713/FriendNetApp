using FriendNetApp.MessagingService.App.Chats.Commands;
using FriendNetApp.MessagingService.App.Chats.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FriendNetApp.MessagingService.Dto;
using FriendNetApp.MessagingService.Services;

namespace FriendNetApp.MessagingService.Controllers
{
    [Route("messaging/chats")]
    public class ChatsController(ILogger<ChatsController> logger,
        IUserAccessor userAccessor,
        GetUserChats.Handler getChats,
        GetChatHistory.Handler getChatHistory,
        Create.Handler createChat,
        SendMessage.Handler sendMessage,
        Delete.Handler deleteChat) : ControllerBase
    {
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<ActionResult<ICollection<ChatDto>>> GetUserChats()
        {
            var curUser = await userAccessor.GetCurrentUserAsync();
            var userId = curUser.Id.ToString();
            var userChats = await getChats.Handle(new GetUserChats.Query { UserId = userId}, CancellationToken.None);
            return Ok(userChats);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<ActionResult<string>> CreateChat([FromBody] CreateChatRequest req)
        {
            var command = new Create.Command
            {
                User1Id = req.User1Id ?? string.Empty,
                User2Id = req.User2Id ?? string.Empty
            };
            try
            {
                string? res = await createChat.Handle(command, CancellationToken.None);
                if (res == null)
                {
                    return StatusCode(500, "Failed to create chat");
                }

                return res;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete/{chatId}")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<ActionResult<string>> DeleteChat(string chatId)
        {
            var command = new Delete.Command
            {
                ChatId = chatId
            };
            try
            {
                string? res = await deleteChat.Handle(command, CancellationToken.None);
                if (res == null)
                {
                    return StatusCode(500, "Failed to delete chat");
                }

                return res;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{chatId}/history")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<ActionResult<ICollection<MessageDto>>> GetChatHistory(string chatId)
        {
            try
            {
                Guid guid = Guid.Parse(chatId);
                var messages = await getChatHistory.Handle(new GetChatHistory.Query
                {
                    ChatId = guid
                }, CancellationToken.None);
                if (messages == null)
                {
                    return NotFound("Chat not found");
                }

                return Ok(messages);
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("send")]
        [Authorize(Roles = "Admin,Client")]
        public async Task<ActionResult> SendMessage([FromBody] MessageDto message)
        {
            var curUser = await userAccessor.GetCurrentUserAsync();
            if (curUser.Id != message.SenderId)
            {
                return Forbid();
            }

            var command = new SendMessage.Command
            {
                Message = message
            };
            try
            {
                string messageId = await sendMessage.Handle(command, CancellationToken.None);
                return Ok(messageId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
