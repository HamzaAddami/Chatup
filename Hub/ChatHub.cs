using System.Collections.Concurrent;
using System.Security.Claims;
using Chatup.DTOs;
using Chatup.Enum;
using Chatup.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatup.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly MessageService _messageService;
    private readonly ConversationService _conversationService;
    private readonly UserService _userService; 
    private readonly ILogger<ChatHub> _logger;

    private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> UserConnections = new();

    public ChatHub(
        MessageService messageService,
        ConversationService conversationService,
        UserService userService, 
        ILogger<ChatHub> logger)
    {
        _messageService = messageService;
        _conversationService = conversationService;
        _userService = userService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId != null)
        {
            try
            {
                UserConnections.AddOrUpdate(
                    userId,
                    new ConcurrentBag<string> { Context.ConnectionId },
                    (key, existingBag) =>
                    {
                        existingBag.Add(Context.ConnectionId);
                        return existingBag;
                    });

                await _userService.UpdateUserOnlineStatusAsync(userId, true);

                var conversations = await _conversationService.GetUserConversationsAsync(userId);
                foreach (var conversation in conversations)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, conversation.Id);
                }

                _logger.LogInformation($"User {userId} connected with connection {Context.ConnectionId}");


                await Clients.Others.SendAsync("UserOnline", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OnConnectedAsync for user {userId}");
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId != null)
        {
            try
            {
                if (UserConnections.TryGetValue(userId, out var connections))
                {
                    var updatedConnections = new ConcurrentBag<string>(
                        connections.Where(c => c != Context.ConnectionId)
                    );

                    if (updatedConnections.IsEmpty)
                    {
                        UserConnections.TryRemove(userId, out _);

                        await _userService.UpdateUserOnlineStatusAsync(userId, false);

                        await Clients.Others.SendAsync("UserOffline", userId, DateTime.UtcNow);
                    }
                    else
                    {
                        UserConnections.TryUpdate(userId, updatedConnections, connections);
                    }
                }

                _logger.LogInformation($"User {userId} disconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OnDisconnectedAsync for user {userId}");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(SendMessageRequest request)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized");
                return;
            }

            var conversation = await _conversationService.GetConversationByIdAsync(request.ConversationId);
            if (conversation == null || !conversation.MemberIds.Contains(userId))
            {
                await Clients.Caller.SendAsync("Error", "You are not a member of this conversation");
                return;
            }

            var message = await _messageService.CreateMessageAsync(
                request.ConversationId,
                userId,
                request.CipherText,
                request.Iv
            );

            await _conversationService.UpdateLastMessageAsync(request.ConversationId, message.Id);

            var messageResponse = await _messageService.MapToResponseAsync(message);

            await Clients.Group(request.ConversationId).SendAsync(
                "ReceiveMessage",
                new MessageNotification(messageResponse, request.ConversationId)
            );

            _logger.LogInformation($"Message {message.Id} sent to conversation {request.ConversationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            await Clients.Caller.SendAsync("Error", "Failed to send message");
        }
    }

    public async Task MarkMessagesAsRead(MarkMessagesReadRequest request)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            var conversation = await _conversationService.GetConversationByIdAsync(request.ConversationId);
            if (conversation == null || !conversation.MemberIds.Contains(userId))
            {
                return;
            }

            await _messageService.MarkMessagesAsReadAsync(request.MessageIds, userId);

            foreach (var messageId in request.MessageIds)
            {
                await Clients.OthersInGroup(request.ConversationId).SendAsync(
                    "MessageRead",
                    new MessageStatusNotification(
                        messageId,
                        request.ConversationId,
                        MessageStatus.READ.ToString(),
                        new List<string> { userId }
                    )
                );
            }

            _logger.LogInformation($"User {userId} marked {request.MessageIds.Count} messages as read");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read");
        }
    }

    public async Task SendTyping(string conversationId, bool isTyping)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "User";

            if (userId == null) return;

            var conversation = await _conversationService.GetConversationByIdAsync(conversationId);
            if (conversation == null || !conversation.MemberIds.Contains(userId))
            {
                return;
            }

            await Clients.OthersInGroup(conversationId).SendAsync(
                "UserTyping",
                new TypingNotification(conversationId, userId, userName, isTyping)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing indicator");
        }
    }

    public async Task JoinConversation(string conversationId)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            var conversation = await _conversationService.GetConversationByIdAsync(conversationId);
            if (conversation == null || !conversation.MemberIds.Contains(userId))
            {
                await Clients.Caller.SendAsync("Error", "You are not a member of this conversation");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
            _logger.LogInformation($"Connection {Context.ConnectionId} joined conversation {conversationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error joining conversation {conversationId}");
        }
    }

    public async Task LeaveConversation(string conversationId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
            _logger.LogInformation($"Connection {Context.ConnectionId} left conversation {conversationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error leaving conversation {conversationId}");
        }
    }
}