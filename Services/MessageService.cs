using Chatup.DTOs;
using Chatup.Entities;
using Chatup.Enum;
using MongoDB.Driver;
using MongoDB.Driver.Core.Servers;

namespace Chatup.Services;


public class MessageService
{
    private readonly IMongoCollection<Message> _messages;
    private readonly IMongoCollection<User> _users;

    public MessageService(IMongoDatabase database)
    {
        _messages = database.GetCollection<Message>("Messages");
        _users = database.GetCollection<User>("Users");
    }

    public async Task<Message> CreateMessageAsync(
        string conversationId, string senderId, string cipherText, string iv)
    {
        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Ciphertext = cipherText,
            IV = iv,
            Status = MessageStatus.SENT
        };

        await _messages.InsertOneAsync(message);
        return message;
    }

    public async Task<List<Message>> GetConversationMessagesAsync(
        string conversationId,
        int limit = 50,
        string? beforeMessageId = null)
    {
        var filterBuilder = Builders<Message>.Filter;
        var filter = filterBuilder.Eq(m => m.ConversationId, conversationId);

        if (!string.IsNullOrEmpty(beforeMessageId))
        {
            var beforeMessage = await _messages.Find(m => m.Id == beforeMessageId).FirstOrDefaultAsync();
            if (beforeMessage != null)
            {
                filter &= filterBuilder.Lt(m => m.CreatedAt, beforeMessage.CreatedAt);
            }
        }

        var sort = Builders<Message>.Sort.Descending(m => m.CreatedAt);

        return await _messages
            .Find(filter)
            .Sort(sort)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task MarkMessageAsReadAsync(string messageId, string userId)
    {
        var update = Builders<Message>.Update
            .AddToSet(m => m.ReadBy, userId)
            .Set(m => m.Status, MessageStatus.READ)
            .Set(m => m.UpdatedAt, DateTime.UtcNow);

        await _messages.UpdateOneAsync(m => m.Id == messageId, update);
    }

    public async Task MarkMessagesAsReadAsync(List<string> messageIds, string userId)
    {
        var filter = Builders<Message>.Filter.In(m => m.Id, messageIds);
        var update = Builders<Message>.Update
            .AddToSet(m => m.ReadBy, userId)
            .Set(m => m.Status, MessageStatus.READ)
            .Set(m => m.UpdatedAt, DateTime.UtcNow);

        await _messages.UpdateManyAsync(filter, update);
    }


    public async Task<int> CountUnreadMessagesAsync(string conversationId, string userId)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId),
            Builders<Message>.Filter.Ne(m => m.SenderId, userId),
            Builders<Message>.Filter.Not(
                Builders<Message>.Filter.AnyEq(m => m.ReadBy, userId)
            )
        );

        return (int)await _messages.CountDocumentsAsync(filter);
    }

    public async Task<MessageResponse> MapToResponseAsync(Message message)
    {
        var sender = await _users.Find(u => u.Id == message.SenderId).FirstOrDefaultAsync();

        return new MessageResponse(
            message.Id,
            message.ConversationId,
            message.SenderId,
            sender?.Nickname ?? sender?.PhoneNumber ?? "Unknown",
            sender?.AvatarUrl,
            message.Ciphertext,
            message.IV,
            message.Status.ToString(),
            message.ReadBy,
            message.CreatedAt
        );
    }





}

