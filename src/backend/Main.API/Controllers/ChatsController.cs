using Main.API.Models;
using Main.Application.Dtos;
using Main.Application.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Main.API.Controllers;

[ApiController]
[Route("api/v1/chats")]
[Authorize]
public class ChatsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<ChatWithUsersDto>> GetUserChats()
    {
        return Ok();
    }

    [HttpPost]
    public ActionResult<SyncChatResponse> CreateChat([FromBody] BaseCreateChatRequest request)
    {
        return Ok();
    }

    [HttpGet("{chatId:guid}")]
    public ActionResult<ChatWithUsersDto> GetChatInfo(Guid chatId)
    {
        return Ok();
    }

    [HttpPost("{chatId:guid}/members")]
    public ActionResult<SyncChatResponse> AddUserToChat(Guid chatId, [FromBody] AddMemberRequest request)
    {
        return Ok();
    }

    [HttpDelete("{chatId:guid}/members/{userId:guid}")]
    public ActionResult<SyncChatResponse> RemoveUserFromChat(Guid chatId, Guid userId)
    {
        return Ok();
    }

    [HttpDelete("{chatId:guid}/members/me")]
    public IActionResult LeaveChat(Guid chatId)
    {
        return NoContent();
    }

    [HttpGet("{chatId:guid}/messages")]
    public ActionResult<List<MessageDto>> GetMessageHistory(Guid chatId, [FromQuery] long? fromMessageNum, [FromQuery] long? limit)
    {
        return Ok();
    }

    [HttpPost("{chatId:guid}/messages")]
    public ActionResult<SyncChatResponse> CreateMessage(Guid chatId, [FromBody] BaseCreateMessageRequest request)
    {
        return Ok();
    }

    [HttpDelete("{chatId:guid}/messages/{messageNum:long}")]
    public ActionResult<SyncChatResponse> DeleteMessage(Guid chatId, long messageNum, [FromQuery] long clientVersion)
    {
        return Ok();
    }

    [HttpPatch("{chatId:guid}/messages/{messageNum:long}")]
    public ActionResult<SyncChatResponse> EditMessage(Guid chatId, long messageNum, [FromBody] EditMessageRequest request)
    {
        return Ok();
    }

    [HttpPost("{chatId:guid}/messages/read")]
    public IActionResult MarkMessagesAsRead(Guid chatId, [FromBody] ReadMessagesRequest request)
    {
        return Ok();
    }

    [HttpPost("sync")]
    public ActionResult<SyncChatsResponse> SyncChats([FromBody] SyncChatsRequest request)
    {
        return Ok();
    }
}
