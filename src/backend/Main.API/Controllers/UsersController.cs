using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Main.API.Models;
using Main.Application.Dtos;
using Main.Application.InPorts;

namespace Main.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IContactService _contactService;
    public UsersController(IUserService userService, IContactService contactService)
    {
        _userService = userService;
        _contactService = contactService;
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
        var result = await _userService.SearchUsersAsync(substr, maxUsers, User.GetUserId());
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
        var result = await _userService.GetUserByIdAsync(id, User.GetUserId());
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
        var result = await _userService.GetMyProfileAsync(User.GetUserId());
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
        var result = await _userService.UpdateDisplayedNameAsync(request.NewDisplayedName, User.GetUserId());
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
        var result = await _contactService.GetMyContactsAsync(User.GetUserId());
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
        var result = await _contactService.AddContactAsync(User.GetUserId(), request.UserContactId, request.ContactName);
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContactWithUserDto>> UpdateContactName(
        Guid contactId,
        [FromBody] UpdateContactNameRequest request)
    {
        var result = await _contactService.ChangeContactNameAsync(User.GetUserId(), contactId, request.NewContactName);
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
        await _contactService.RemoveContactAsync(User.GetUserId(), contactId);
        return NoContent();
    }
}
