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
public class KeysController : ControllerBase
{
    private readonly IMongoCollection<UserKey> _userKeys;

    public KeysController(IMongoDatabase database)
    {
        _userKeys = database.GetCollection<UserKey>("UserKeys");
    }

    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterPublicKey([FromBody] RegisterPublicKeyRequest request)
    {
        try
        {
            var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (myId == null) return Unauthorized();

            var existingKey = await _userKeys.Find(k => k.UserId == myId).FirstOrDefaultAsync();

            if (existingKey != null)
            {
                // update key
                var update = Builders<UserKey>.Update.Set(k => k.PublicKey, request.PublicKey);
                await _userKeys.UpdateOneAsync(k => k.UserId == myId, update);
            }
            else
            {
                // create new key
                var userKey = new UserKey
                {
                    UserId = myId,
                    PublicKey = request.PublicKey
                };
                await _userKeys.InsertOneAsync(userKey);
            }

            return Ok(new { message = "Public key registered successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPublicKey(string userId)
    {
        try
        {
            var userKey = await _userKeys.Find(k => k.UserId == userId).FirstOrDefaultAsync();

            if (userKey == null)
            {
                return NotFound(new { error = "Public key not found for this user" });
            }

            return Ok(new PublicKeyResponse(userKey.UserId, userKey.PublicKey));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

   
     [HttpPost("bulk")]
    public async Task<IActionResult> GetBulkPublicKeys([FromBody] List<string> userIds)
    {
        try
        {
            var filter = Builders<UserKey>.Filter.In(k => k.UserId, userIds);
            var keys = await _userKeys.Find(filter).ToListAsync();

            var response = keys.Select(k => new PublicKeyResponse(k.UserId, k.PublicKey)).ToList();
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    
    [HttpGet("my-key")]
    public async Task<IActionResult> GetMyPublicKey()
    {
        try
        {
            var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (myId == null) return Unauthorized();

            var userKey = await _userKeys.Find(k => k.UserId == myId).FirstOrDefaultAsync();
            
            if (userKey == null)
            {
                return NotFound(new { error = "You haven't registered a public key yet" });
            }

            return Ok(new PublicKeyResponse(userKey.UserId, userKey.PublicKey));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}