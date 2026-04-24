using Main.API.Models;
using Main.Application.Dtos;
using Main.Application.Exceptions;
using Main.Application.InPorts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Main.API.Controllers;

[ApiController]
[Route("api/v1/chats")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IMessageService _messageService;
    private readonly ISyncService _syncService;
    private readonly ILogger _logger;
    public ChatsController(IChatService chatService, IMessageService messageService,
        ISyncService syncService, ILogger logger)
    {
        _chatService = chatService;
        _messageService = messageService;
        _syncService = syncService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ChatWithUsersDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ChatWithUsersDto>>> GetUserChats()
    {
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} getting chats", userId);
        var result = await _chatService.GetChatsAsync(userId);
        _logger.LogInformation("User {userId} successfully retrieved {count} chats", userId, result.Count());
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
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} creating {type} chat", userId, request.ChatType.ToString());
        switch (request)
        {
            case CreatePrivateChatRequest privateChat:
                chatId = await _chatService.CreatePrivateChatAsync(privateChat.WithUserId, userId);
                break;

            case CreateGroupChatRequest groupChat:
                chatId = await _chatService.CreateGroupChatAsync(groupChat.ChatName, groupChat.MemberIds, userId);
                break;

            default:
                throw new RuleViolationException($"Unknown chat type: {request.GetType().Name}");
        }
        var sync = await _syncService.SyncChatAsync(chatId, 0, userId);
        var response = new SyncChatResponse { Chat = sync };
        _logger.LogInformation("User {userId} successfully created {type} chat", userId, request.ChatType.ToString());
        return CreatedAtAction(nameof(GetChatInfo), new { chatId }, response);
    }

    [HttpGet("{chatId:guid}")]
    [ProducesResponseType(typeof(ChatWithUsersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChatWithUsersDto>> GetChatInfo(Guid chatId)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} getting chat {chatId} info", userId, chatId);
        var result = await _chatService.GetChatByIdAsync(chatId, userId);
        _logger.LogInformation("User {userId} successfully got chat {chatId} info", userId, chatId);
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
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} updating chat {chatId} name", userId, chatId);
        if (request.ClientVersion < 0)
            throw new ArgumentException("Client version must be non-negative");
        await _chatService.UpdateChatNameAsync(chatId, request.NewChatName, userId);
        var result = await _syncService.SyncChatAsync(chatId, (ulong) request.ClientVersion, userId);
        _logger.LogInformation("User {userId} successfully updated chat {chatId} name", userId, chatId);
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
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} adding user {toadd} into chat {chatId}", userId, request.UserId, chatId);
        if (request.ClientVersion < 0)
            throw new ArgumentException("Client version must be non-negative");
        await _chatService.AddMemberAsync(chatId, request.UserId, userId);
        var result = await _syncService.SyncChatAsync(chatId, (ulong) request.ClientVersion, userId);
        _logger.LogInformation("User {userId} successfully added user {toadd} into chat {chatId}", userId, 
            request.UserId, chatId);
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
        var id = User.GetUserId();
        _logger.LogInformation("User {userId} removing user {torm} from chat {chatId}", id, userId, chatId);
        if (clientVersion < 0)
            throw new ArgumentException("Client version must be non-negative");
        await _chatService.RemoveMemberAsync(chatId, userId, id);
        var result = await _syncService.SyncChatAsync(chatId, (ulong) clientVersion, id);
        _logger.LogInformation("User {userId} successfully removed user {torm} from chat {chatId}", id, userId, chatId);
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
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} leaving chat {chatId}", userId, chatId);
        await _chatService.LeaveChatAsync(chatId, userId);
        _logger.LogInformation("User {userId} successfully left chat {chatId}", userId, chatId);
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
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} getting message history from chat {chatId}", userId, chatId);
        if (fromMessageNum < 0 || limit <= 0)
            throw new ArgumentException("fromMessageNum >= 0 and limit > 0");
        var result = await _messageService.GetOlderMessagesAsync(chatId, (ulong) fromMessageNum, limit, userId);
        _logger.LogInformation("User {userId} successfully got {cnt} messages from chat {chatId}",
            userId, result.Count(), chatId);
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
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} sending {type} message into chat {chatId}", userId, 
            request.MessageType.ToString(), chatId);
        if (request.ClientVersion < 0)
            throw new ArgumentException("ClientVersion >= 0");
        switch (request)
        {
            case SendMessageRequest regularMessage:
                await _messageService.CreateRegularMessageAsync(chatId, userId, regularMessage.Text);
                break;

            case ReplyMessageRequest replyMessage:
                if (replyMessage.ReplyToMessageNum < 0)
                    throw new ArgumentException("replyToMessageNum >= 0");
                await _messageService.CreateReplyMessageAsync(chatId, userId, 
                    replyMessage.Text, (ulong) replyMessage.ReplyToMessageNum);
                break;

            case ForwardMessageRequest forwardMessage:
                if (forwardMessage.ForwardedFromMessageNum < 0)
                    throw new ArgumentException("ForwardedFromMessageNum >= 0");
                await _messageService.CreateForwardMessageAsync(chatId, forwardMessage.ForwardedFromChatId, 
                    (ulong) forwardMessage.ForwardedFromMessageNum, userId);
                break;

            default:
                throw new RuleViolationException($"Unknown message type: {request.GetType().Name}");
        }
        var sync = await _syncService.SyncChatAsync(chatId, (ulong) request.ClientVersion, userId);
        var result = new SyncChatResponse { Chat = sync };
        _logger.LogInformation("User {userId} successfully sent {type} message into chat {chatId}", 
            userId, request.MessageType.ToString(), chatId);
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
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} deleting message {messageNum} in chat {chatId}", userId, messageNum, chatId);
        if (messageNum < 0 || clientVersion < 0)
            throw new ArgumentException("MessageNum >= 0 and ClientVersion >= 0");
        await _messageService.DeleteMessageAsync(chatId, (ulong) messageNum, userId);
        var sync = await _syncService.SyncChatAsync(chatId, (ulong) clientVersion, userId);
        _logger.LogInformation("User {userId} successfully deleted message {messageNum} from chat {chatId}", 
            userId, messageNum, chatId);
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
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} editing message {messageNum} in chat {chatId}", userId, messageNum, chatId);
        if (messageNum < 0)
            throw new ArgumentException("MessageNum >= 0");
        await _messageService.EditMessageAsync(chatId, (ulong) messageNum, request.NewText, userId);
        var sync = await _syncService.SyncChatAsync(chatId, (ulong) request.ClientVersion, userId);
        _logger.LogInformation("User {userId} successfully edited message {messageNum} in chat {chatId}", 
            userId, messageNum, chatId);
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
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} reading messages up to {messageNum} in chat {chatId}", 
            userId, request.LastMessageNum, chatId);
        if (request.LastMessageNum < 0)
            throw new ArgumentException("MessageNum >= 0");
        await _messageService.MarkMessagesAsReadAsync(chatId, (ulong) request.LastMessageNum, userId);
        _logger.LogInformation("User {userId} successfully read messages up to {messageNum} in chat {chatId}",
            userId, request.LastMessageNum, chatId);
        return NoContent();
    }

    /// <summary>
    /// Синхронизировать один чат
    /// </summary>
    /// <param name="chatId">ID чата для синхронизации</param>
    /// <param name="request">Версия клиента</param>
    /// <returns>Обновлённые данные чата</returns>
    [HttpPost("sync/{chatId:guid}")]
    [ProducesResponseType(typeof(SyncChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatResponse>> SyncChat(
        Guid chatId,
        [FromBody] SyncChatRequest request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} sync chat {chatId}", userId, chatId);
        if (request.ClientVersion < 0)
            throw new RuleViolationException("Client version must be non-negative");
        var sync = await _syncService.SyncChatAsync(chatId, (ulong)request.ClientVersion, userId);
        _logger.LogInformation(
            "User {userId} successfully sync chat {chatId}: status={status}", userId,
            chatId, sync.Status);
        return Ok(new SyncChatResponse { Chat = sync });
    }

    [HttpPost("sync")]
    [ProducesResponseType(typeof(SyncChatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SyncChatsResponse>> SyncChats([FromBody] SyncChatsRequest request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("User {userId} sync chats", userId);
        if (request.Chats.Any(c => c.ClientVersion < 0))
            throw new ArgumentException("ClientVersion >= 0");
        var result = await _syncService.SyncChatsAsync(
            request.Chats.Select(c => (c.ChatId, (ulong) c.ClientVersion)).ToList(), userId);
        _logger.LogInformation("User {userId} successfully sync chats", userId);
        return Ok(new SyncChatsResponse { Chats = result });
    }
}
