using Main.Application.InPorts;
using Main.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Main.API.Controllers;

[ApiController]
[Route("api/v1/internal/chats")]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class InternalChatsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger _logger;
    public InternalChatsController(IChatService chatService, ILogger logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Получить ID чатов пользователя
    /// </summary>
    [HttpGet("by-user/{userId:guid}")]
    [ProducesResponseType(typeof(List<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Guid>>> GetUserChatIds(Guid userId)
    {
        _logger.LogInformation("Internal: getting chat IDs for user {UserId} (internal call)", userId);
        var chatIds = await _chatService.GetChatsIdsAsync(userId);
        _logger.LogInformation("Internal: retrieved {Count} chat IDs for user {UserId}",
            chatIds.Count(), userId);
        return Ok(chatIds);
    }
}