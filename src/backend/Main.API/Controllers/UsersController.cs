using Main.API.Models;
using Main.Application.Dtos;
using Main.Application.InPorts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Main.API.Controllers;

using ILogger = Serilog.ILogger;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IContactService _contactService;
    private readonly ILogger _logger;
    public UsersController(IUserService userService, IContactService contactService, ILogger logger)
    {
        _userService = userService;
        _contactService = contactService;
        _logger = logger;
    }

    /// <summary>
    /// Поиск пользователей по подстроке в отображаемом имени
    /// </summary>
    /// <param name="substr">Подстрока</param>
    /// <param name="maxUsers">Максимальное количество пользователей, которое нужно вернуть</param>
    /// <returns>Список пользователей, чье отображаемое имя содержит подстроку</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string substr, [FromQuery] int maxUsers)
    {
        var userId = User.GetUserId();
        _logger.Information("User {userId} searching users by substring {sub}", userId, substr);
        var result = await _userService.SearchUsersAsync(substr, maxUsers, userId);
        _logger.Information("User {userId} successfully found {cnt} users by substring {sub}", 
            userId, result.Count(), substr);
        return Ok(result);
    }

    /// <summary>
    /// Получение пользователя по Id
    /// </summary>
    /// <param name="id">Id искомого пользователя</param>
    /// <returns>Информация о пользователе</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var userId = User.GetUserId();
        _logger.Information("User {userId} getting user {id} info", userId, id);
        var result = await _userService.GetUserByIdAsync(id, userId);
        _logger.Information("User {userId} successfully got user {id} info", userId, id);
        return Ok(result);
    }

    /// <summary>
    /// Получить информацию о себе (текущем пользователе)
    /// </summary>
    /// <returns>Информация о пользователе</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetMyProfile()
    {
        var userId = User.GetUserId();
        _logger.Information("User {userId} getting his profile info", userId);
        var result = await _userService.GetMyProfileAsync(userId);
        _logger.Information("User {userId} successfully got his profile info", userId);
        return Ok(result);
    }

    /// <summary>
    /// Обновить свое отображаемое имя
    /// </summary>
    /// <param name="request">Новое отображаемое имя</param>
    /// <returns>Информация о пользователе</returns>
    [HttpPatch("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> UpdateDisplayedName([FromBody] UpdateDisplayedNameRequest request)
    {
        var userId = User.GetUserId();
        _logger.Information("User {userId} updating his displayed name", userId);
        var result = await _userService.UpdateDisplayedNameAsync(request.NewDisplayedName, userId);
        _logger.Information("User {userId} successfully updated his displayed name", userId);
        return Ok(result);
    }

    /// <summary>
    /// Получить свои контакты
    /// </summary>
    /// <returns>Список контактов</returns>
    [HttpGet("me/contacts")]
    [ProducesResponseType(typeof(List<ContactWithUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ContactWithUserDto>>> GetContacts()
    {
        var userId = User.GetUserId();
        _logger.Information("User {userId} gettig his contacts", userId);
        var result = await _contactService.GetMyContactsAsync(userId);
        _logger.Information("User {userId} successfully got {cnt} contacts", userId, result.Count());
        return Ok(result);
    }

    /// <summary>
    /// Добавить новый контакт
    /// </summary>
    /// <param name="request">Информация о контакте</param>
    /// <returns>Созданный контакт</returns>
    [HttpPost("me/contacts")]
    [ProducesResponseType(typeof(ContactWithUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContactWithUserDto>> AddContact([FromBody] AddContactRequest request)
    {
        var userId = User.GetUserId();
        _logger.Information("User {userId} adding contact with user {id}", userId, request.UserContactId);
        var result = await _contactService.AddContactAsync(userId, request.UserContactId, request.ContactName);
        _logger.Information("User {userId} successfully added contact with user {id}", userId, request.UserContactId);
        return CreatedAtAction(nameof(AddContact), result);
    }

    /// <summary>
    /// Обновить имя контакта
    /// </summary>
    /// <param name="contactId">Id пользователя-контакта</param>
    /// <param name="request">Новое имя контакта</param>
    /// <returns>Обновленный контакт</returns>
    [HttpPatch("me/contacts/{contactId:guid}")]
    [ProducesResponseType(typeof(ContactWithUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContactWithUserDto>> UpdateContactName(
        Guid contactId,
        [FromBody] UpdateContactNameRequest request)
    {
        var userId = User.GetUserId();
        _logger.Information("User {userId} changing contact name with user {id}", userId, contactId);
        var result = await _contactService.ChangeContactNameAsync(userId, contactId, request.NewContactName);
        _logger.Information("User {userId} successfully changed contact name with user {id}", userId, contactId);
        return Ok(result);
    }

    /// <summary>
    /// Удалить контакт
    /// </summary>
    /// <param name="contactId">Id пользователя-контакта</param>
    /// <returns>No content</returns>
    [HttpDelete("me/contacts/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveContact(Guid contactId)
    {
        var userId = User.GetUserId();
        _logger.Information("User {userId} removing contact with user {id}", userId, contactId);
        await _contactService.RemoveContactAsync(userId, contactId);
        _logger.Information("User {userId} successfully removed contact with user {id}", userId, contactId);
        return NoContent();
    }
}
