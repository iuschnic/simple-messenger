using Main.API.Models;
using Main.Application.Dtos;
using Main.Application.Exceptions;
using Main.Application.InPorts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Main.API.Controllers;

[ApiController]
[Route("api/v1/chats")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IMessageService _messageService;
    private readonly ISyncService _syncService;
    public ChatsController(IChatService chatService, IMessageService messageService, ISyncService syncService)
    {
        _chatService = chatService;
        _messageService = messageService;
        _syncService = syncService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ChatWithUsersDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ChatWithUsersDto>>> GetUserChats()
    {
        var result = await _chatService.GetChatsAsync(User.GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Создать новый чат (приватный или групповой)
    /// </summary>
    /// <param name="request">Запрос на создание чата (полиморфизм по ChatType)</param>
    /// <returns>Sync со всей информацией о чате</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SyncChatResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatResponse>> CreateChat([FromBody] BaseCreateChatRequest request)
    {
        Guid chatId;
        switch (request)
        {
            case CreatePrivateChatRequest privateChat:
                chatId = await _chatService.CreatePrivateChatAsync(privateChat.WithUserId, User.GetUserId());
                break;

            case CreateGroupChatRequest groupChat:
                chatId = await _chatService.CreateGroupChatAsync(groupChat.ChatName, groupChat.MemberIds, User.GetUserId());
                break;

            default:
                throw new RuleViolationException($"Unknown chat type: {request.GetType().Name}");
        }
        var sync = await _syncService.SyncChatAsync(chatId, 0, User.GetUserId());
        var response = new SyncChatResponse { Chat = sync };
        return CreatedAtAction(nameof(GetChatInfo), new { chatId }, response);
    }

    [HttpGet("{chatId:guid}")]
    [ProducesResponseType(typeof(ChatWithUsersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChatWithUsersDto>> GetChatInfo(Guid chatId)
    {
        var result = await _chatService.GetChatByIdAsync(chatId, User.GetUserId());
        return Ok(result);
    }

    [HttpPatch("{chatId:guid}")]
    [ProducesResponseType(typeof(SyncChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatResponse>> UpdateChatName(Guid chatId, [FromBody] UpdateChatNameRequest request)
    {
        if (request.ClientVersion < 0)
            return BadRequest("Client version must be non-negative");
        await _chatService.UpdateChatNameAsync(chatId, request.NewChatName, User.GetUserId());
        var result = await _syncService.SyncChatAsync(chatId, (ulong) request.ClientVersion, User.GetUserId());
        return Ok(new SyncChatResponse { Chat = result });
    }

    [HttpPost("{chatId:guid}/members")]
    [ProducesResponseType(typeof(SyncChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatResponse>> AddUserToChat(Guid chatId, [FromBody] AddMemberRequest request)
    {
        if (request.ClientVersion < 0)
            return BadRequest("Client version must be non-negative");
        await _chatService.AddMemberAsync(chatId, request.UserId, User.GetUserId());
        var result = await _syncService.SyncChatAsync(chatId, (ulong) request.ClientVersion, User.GetUserId());
        return Ok(new SyncChatResponse { Chat = result });
    }

    [HttpDelete("{chatId:guid}/members/{userId:guid}")]
    [ProducesResponseType(typeof(SyncChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatResponse>> RemoveUserFromChat(Guid chatId, Guid userId, [FromQuery] long clientVersion)
    {
        if (clientVersion < 0)
            return BadRequest("Client version must be non-negative");
        await _chatService.RemoveMemberAsync(chatId, userId, User.GetUserId());
        var result = await _syncService.SyncChatAsync(chatId, (ulong) clientVersion, User.GetUserId());
        return Ok(new SyncChatResponse { Chat = result });
    }

    [HttpDelete("{chatId:guid}/members/me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LeaveChat(Guid chatId)
    {
        await _chatService.LeaveChatAsync(chatId, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{chatId:guid}/messages")]
    [ProducesResponseType(typeof(List<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<MessageDto>>> GetMessageHistory(Guid chatId, 
        [FromQuery] long fromMessageNum, [FromQuery] int limit)
    {
        if (fromMessageNum < 0 || limit <= 0)
            return BadRequest("fromMessageNum >= 0 and limit > 0");
        var result = await _messageService.GetOlderMessagesAsync(chatId, (ulong) fromMessageNum, limit, User.GetUserId());
        return Ok(result);
    }

    [HttpPost("{chatId:guid}/messages")]
    [ProducesResponseType(typeof(SyncChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatResponse>> CreateMessage(Guid chatId, [FromBody] BaseCreateMessageRequest request)
    {
        if (request.ClientVersion < 0)
            return BadRequest("ClientVersion >= 0");
        switch (request)
        {
            case SendMessageRequest regularMessage:
                await _messageService.CreateRegularMessageAsync(chatId, User.GetUserId(), regularMessage.Text);
                break;

            case ReplyMessageRequest replyMessage:
                if (replyMessage.ReplyToMessageNum < 0)
                    return BadRequest("replyToMessageNum >= 0");
                await _messageService.CreateReplyMessageAsync(chatId, User.GetUserId(), 
                    replyMessage.Text, (ulong) replyMessage.ReplyToMessageNum);
                break;

            case ForwardMessageRequest forwardMessage:
                if (forwardMessage.ForwardedFromMessageNum < 0)
                    return BadRequest("ForwardedFromMessageNum >= 0");
                await _messageService.CreateForwardMessageAsync(chatId, forwardMessage.ForwardedFromChatId, 
                    (ulong) forwardMessage.ForwardedFromMessageNum, User.GetUserId());
                break;

            default:
                throw new RuleViolationException($"Unknown message type: {request.GetType().Name}");
        }
        var sync = await _syncService.SyncChatAsync(chatId, (ulong) request.ClientVersion, User.GetUserId());
        var result = new SyncChatResponse { Chat = sync };
        return Ok(result);
    }

    [HttpDelete("{chatId:guid}/messages/{messageNum:long}")]
    [ProducesResponseType(typeof(SyncChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatResponse>> DeleteMessage(Guid chatId, long messageNum, [FromQuery] long clientVersion)
    {
        if (messageNum < 0 || clientVersion < 0)
            return BadRequest("MessageNum >= 0 and ClientVersion >= 0");
        await _messageService.DeleteMessageAsync(chatId, (ulong) messageNum, User.GetUserId());
        var sync = await _syncService.SyncChatAsync(chatId, (ulong) clientVersion, User.GetUserId());
        var result = new SyncChatResponse { Chat = sync };
        return Ok(result);
    }

    [HttpPatch("{chatId:guid}/messages/{messageNum:long}")]
    [ProducesResponseType(typeof(SyncChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatResponse>> EditMessage(Guid chatId, long messageNum, [FromBody] EditMessageRequest request)
    {
        if (messageNum < 0)
            return BadRequest("MessageNum >= 0");
        await _messageService.EditMessageAsync(chatId, (ulong) messageNum, request.NewText, User.GetUserId());
        var sync = await _syncService.SyncChatAsync(chatId, (ulong) request.ClientVersion, User.GetUserId());
        var result = new SyncChatResponse { Chat = sync };
        return Ok(result);
    }

    [HttpPost("{chatId:guid}/messages/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkMessagesAsRead(Guid chatId, [FromBody] ReadMessagesRequest request)
    {
        if (request.LastMessageNum < 0)
            return BadRequest("MessageNum >= 0");
        await _messageService.MarkMessagesAsReadAsync(chatId, (ulong) request.LastMessageNum, User.GetUserId());
        return NoContent();
    }

    [HttpPost("sync")]
    [ProducesResponseType(typeof(SyncChatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatsResponse>> SyncChats([FromBody] SyncChatsRequest request)
    {
        if (request.Chats.Any(c => c.ClientVersion < 0))
            return BadRequest("ClientVersion >= 0");
        var result = await _syncService.SyncChatsAsync(
            request.Chats.Select(c => (c.ChatId, (ulong) c.ClientVersion)).ToList(), User.GetUserId());
        return Ok(new SyncChatsResponse { Chats = result });
    }
}
