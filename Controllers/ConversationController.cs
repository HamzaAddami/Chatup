using System.Security.Claims;
using Chatup.DTOs;
using Chatup.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chatup.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationController : ControllerBase
{
    private readonly ConversationService _conversationService;
    private readonly MessageService _messageService;

    public ConversationController(
        ConversationService conversationService,
        MessageService messageService)
    {
        _conversationService = conversationService;
        _messageService = messageService;
    }

  
    [HttpPost("private")]
    public async Task<IActionResult> CreatePrivateConversation([FromBody] CreatePrivateConversationRequest request)
    {
        try
        {
            var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (myId == null) return Unauthorized();

            var conversation = await _conversationService.CreatePrivateConversationAsync(
                myId, 
                request.MyPublicKey,
                request.ContactUserId     
            );

            return Ok(new { conversationId = conversation.Id});
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

  
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        try
        {
            var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (myId == null) return Unauthorized();

            var conversation = await _conversationService.CreateGroupAsync(
                myId,
                request.GroupName,
                request.MemberIds,
                request.MembersPublicKeys,
                request.GroupAvatarUrl
            );

            return Ok(new { conversationId = conversation.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

   
    [HttpGet]
    public async Task<IActionResult> GetMyConversations()
    {
        try
        {
            var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (myId == null) return Unauthorized();

            var conversations = await _conversationService.GetUserConversationsAsync(myId);

            var responses = new List<ConversationResponse>();

            foreach (var conv in conversations)
            {
                var members = await _conversationService.GetConversationMembersAsync(conv.Id);

                MessageResponse? lastMessage = null;
                if (!string.IsNullOrEmpty(conv.LastMessageId))
                {
                    var messages = await _messageService.GetConversationMessagesAsync(conv.Id, 1);
                    if (messages.Any())
                    {
                        lastMessage = await _messageService.MapToResponseAsync(messages[0]);
                    }
                }

                var unreadCount = await _messageService.CountUnreadMessagesAsync(conv.Id, myId);



                responses.Add(new ConversationResponse(
                    conv.Id,
                    conv.Type.ToString(),
                    conv.GroupName,
                    conv.GroupAvatarUrl,
                    members,
                    lastMessage,
                    conv.LastMessageAt,
                    unreadCount
                ));
            }

            return Ok(responses);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    
    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetConversation(string conversationId)
    {
        try
        {
            var myId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (myId == null) return Unauthorized();

            var conversations = await _conversationService.GetUserConversationsAsync(myId);
            var conv = conversations.FirstOrDefault(c => c.Id == conversationId);

            if (conv == null) return NotFound();

            var members = await _conversationService.GetConversationMembersAsync(conv.Id);
            var unreadCount = await _messageService.CountUnreadMessagesAsync(conv.Id, myId);

            MessageResponse? lastMessage = null;
            if (!string.IsNullOrEmpty(conv.LastMessageId))
            {
                var messages = await _messageService.GetConversationMessagesAsync(conv.Id, 1);
                if (messages.Any())
                {
                    lastMessage = await _messageService.MapToResponseAsync(messages[0]);
                }
            }

            var response = new ConversationResponse(
                conv.Id,
                conv.Type.ToString(),
                conv.GroupName,
                conv.GroupAvatarUrl,
                members,
                lastMessage,
                conv.LastMessageAt,
                unreadCount
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}