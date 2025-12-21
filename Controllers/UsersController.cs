using System.Security.Claims;
using Chatup.DTOs;
using Chatup.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Chatup.Controllers;

[ApiController]
[Route("api/[controller]")]
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
}