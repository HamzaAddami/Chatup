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
    private readonly IMongoCollection<User> _users;

    public UsersController(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("Users");
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _users.Find(u => u.Id == myId).FirstOrDefaultAsync();

        if (user == null) return NotFound();

        return Ok(new UserResponse(user.Id, user.PhoneNumber, user.Nickname, user.About, user.AvatarUrl));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var update = Builders<User>.Update
            .Set(u => u.Nickname, request.Nickname)
            .Set(u => u.About, request.About)
            .Set(u => u.AvatarUrl, request.AvatarUrl);
        
        await _users.UpdateOneAsync(u => u.Id == myId, update);
        
        return Ok();
    }

    [HttpPost("contacts")]
    public async Task<IActionResult> AddContact([FromBody] AddContactRequest request)
    {
        var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var contactUser = await _users.Find(u => u.PhoneNumber == request.PhoneNumber).FirstOrDefaultAsync();
        if (contactUser == null)
        {
            return BadRequest(new { error = "User with this phone number not found." });
        }

        if (contactUser.Id == myId)
        {
            return BadRequest(new { error = "You cannot add yourself as a contact." });
        }

        var update = Builders<User>.Update.AddToSet(u => u.ContactIds, contactUser.Id);
        await _users.UpdateOneAsync(u => u.Id == myId, update);
        
        return Ok();
    }

    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts()
    {
        var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var me = await _users.Find(u => u.Id == myId).FirstOrDefaultAsync();
        if (me == null || me.ContactIds.Count == 0) return Ok(new List<UserResponse>());

        // get all users whose IDs are in my ContactsIds list
        var filter = Builders<User>.Filter.In(u => u.Id, me.ContactIds);
        
        var contacts = await _users.Find(filter).ToListAsync();

        // map to DTOs
        var response = contacts.Select(u =>
            new UserResponse(u.Id, u.PhoneNumber, u.Nickname, u.About, u.AvatarUrl)
        );

        return Ok(response);
    }

    [HttpPost("block")]
    public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
    {
        var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var userToBlock = await _users.Find(u => u.PhoneNumber == request.PhoneNumber).FirstOrDefaultAsync();
        if (userToBlock == null)
        {
            return BadRequest(new { error = "User with this phone number not found." });
        }

        if (userToBlock.Id == myId)
        {
            return BadRequest(new { error = "You cannot block yourself." });
        }
        
        var update = Builders<User>.Update.AddToSet(u => u.BlockedUserIds, userToBlock.Id);
        await _users.UpdateOneAsync(u => u.Id == myId, update);
        
        return Ok();
    }

    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlocked()
    {
        var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var me = await _users.Find(u => u.Id == myId).FirstOrDefaultAsync();
        if (me == null || me.BlockedUserIds.Count == 0) return Ok(new List<UserResponse>());

        var filter = Builders<User>.Filter.In(u => u.Id, me.BlockedUserIds);

        var blocked = await _users.Find(filter).ToListAsync();

        var response = blocked.Select(u =>
            new UserResponse(u.Id, u.PhoneNumber, u.Nickname, u.About, u.AvatarUrl)
        );

        return Ok(response);
    }
    
    [HttpPost("unblock")]
    public async Task<IActionResult> UnblockUser([FromBody] BlockUserRequest request)
    {
        var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userToBlock = await _users.Find(u => u.PhoneNumber == request.PhoneNumber).FirstOrDefaultAsync();
        
        if (userToBlock == null) return NotFound();

        var update = Builders<User>.Update.Pull(u => u.BlockedUserIds, userToBlock.Id);
        await _users.UpdateOneAsync(u => u.Id == myId, update);
        
        return Ok();
    }
}