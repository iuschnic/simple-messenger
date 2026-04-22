using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Main.API.Models;
using Main.Application.Dtos;
using Main.Application.InPorts;
using Main.Application.Exceptions;
using Main.Application.Services;

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
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string substr, [FromQuery] int maxUsers)
    {
        try
        {
            var result = await _userService.SearchUsersAsync(substr, maxUsers, User.GetUserId());
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        try
        {
            var result = await _userService.GetUserByIdAsync(id, User.GetUserId());
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMyProfile()
    {
        try
        {
            var result = await _userService.GetMyProfileAsync(User.GetUserId());
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpPatch("me")]
    public async Task<ActionResult<UserDto>> UpdateDisplayedName([FromBody] UpdateDisplayedNameRequest request)
    {
        try
        {
            var result = await _userService.UpdateDisplayedNameAsync(request.NewDisplayedName, User.GetUserId());
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpGet("me/contacts")]
    public async Task<ActionResult<List<ContactWithUserDto>>> GetContacts()
    {
        try
        {
            var result = await _contactService.GetMyContactsAsync(User.GetUserId());
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpPost("me/contacts")]
    public async Task<ActionResult<ContactWithUserDto>> AddContact([FromBody] AddContactRequest request)
    {
        try
        {
            var result = await _contactService.AddContactAsync(User.GetUserId(), request.UserContactId, request.ContactName);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpPatch("me/contacts/{contactId:guid}")]
    public async Task<ActionResult<ContactWithUserDto>> UpdateContactName(Guid contactId, [FromBody] UpdateContactNameRequest request)
    {
        try
        {
            var result = await _contactService.ChangeContactNameAsync(User.GetUserId(), contactId, request.NewContactName);
            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpDelete("me/contacts/{contactId:guid}")]
    public async Task<IActionResult> RemoveContact(Guid contactId)
    {
        try
        {
            await _contactService.RemoveContactAsync(User.GetUserId(), contactId);
            return NoContent();
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }
}
