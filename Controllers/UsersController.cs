using System.Security.Claims;
using Chatup.DTOs;
using Chatup.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Chatup.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await _userService.GetUserByIdAsync(CurrentUserId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        await _userService.UpdateProfileAsync(CurrentUserId, request);
        return Ok(new { message = "Profile updated successfully" });
    }

    [HttpPost("contacts")]
    public async Task<IActionResult> AddContact([FromBody] AddContactRequest request)
    {
        var result = await _userService.AddContactAsync(CurrentUserId, request.PhoneNumber);

        if (!result.Success) return BadRequest(new { error = result.Message });

        return Ok(new { message = result.Message });
    }

    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts()
    {
        var contacts = await _userService.GetContactsAsync(CurrentUserId);
        return Ok(contacts);
    }

    [HttpPost("block")]
    public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
    {
        var result = await _userService.BlockUserAsync(CurrentUserId, request.PhoneNumber);

        if (!result.Success) return BadRequest(new { error = result.Message });

        return Ok(new { message = result.Message });
    }

    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlocked()
    {
        var blockedUsers = await _userService.GetBlockedUsersAsync(CurrentUserId);
        return Ok(blockedUsers);
    }

    [HttpPost("unblock")]
    public async Task<IActionResult> UnblockUser([FromBody] BlockUserRequest request)
    {
        var result = await _userService.UnblockUserAsync(CurrentUserId, request.PhoneNumber);

        if (!result.Success) return NotFound(new { error = result.Message });

        return Ok(new { message = result.Message });
    }
}