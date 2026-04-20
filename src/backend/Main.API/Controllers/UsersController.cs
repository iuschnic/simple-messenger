using Main.API.Models;
using Main.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Main.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<UserDto>> SearchUsers([FromQuery] string? substr, [FromQuery] long? maxUsers)
    {
        return Ok();
    }

    [HttpGet("{id:guid}")]
    public ActionResult<UserDto> GetUserById(Guid id)
    {
        return Ok();
    }

    [HttpGet("me")]
    public ActionResult<UserDto> GetMyProfile()
    {
        return Ok();
    }

    [HttpPatch("me")]
    public ActionResult<UserDto> UpdateDisplayedName([FromBody] UpdateDisplayedNameRequest request)
    {
        return Ok();
    }

    [HttpGet("me/contacts")]
    public ActionResult<List<ContactWithUserDto>> GetContacts()
    {
        return Ok();
    }

    [HttpPost("me/contacts")]
    public ActionResult<ContactWithUserDto> AddContact([FromBody] AddContactRequest request)
    {

        return Ok();
    }

    [HttpPatch("me/contacts/{contactId:guid}")]
    public ActionResult<ContactWithUserDto> UpdateContactName(Guid contactId, [FromBody] UpdateContactNameRequest request)
    {
        return Ok();
    }

    [HttpDelete("me/contacts/{contactId:guid}")]
    public IActionResult RemoveContact(Guid contactId)
    {
        return NoContent();
    }
}
