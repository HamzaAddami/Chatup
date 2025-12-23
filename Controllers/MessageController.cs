using System.Security.Claims;
using Chatup.DTOs;
using Chatup.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chatup.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly MessageService _messageService;

    public MessagesController(MessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("conversation/{conversationId}")]
    public async Task<IActionResult> GetMessages(
        string conversationId,
        [FromQuery] int limit = 50,
        [FromQuery] string? before = null)
    {
        try
        {
            var messages = await _messageService.GetConversationMessagesAsync(
                conversationId,
                limit,
                before
            );

            var responses = new List<MessageResponse>();
            foreach (var msg in messages)
            {
                responses.Add(await _messageService.MapToResponseAsync(msg));
            }

            return Ok(responses);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Mark a msg as read
    [HttpPost("{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(string messageId)
    {
        try
        {
            var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (myId == null) return Unauthorized();

            await _messageService.MarkMessageAsReadAsync(messageId, myId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

