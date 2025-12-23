using Chatup.Entities;
using MongoDB.Driver;
using Chatup.Enum;
using Chatup.DTOs;

namespace Chatup.Services;



public class ConversationService
{
    private readonly IMongoCollection<Conversation> _conversations;
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<UserKey> _keys;

    public ConversationService(IMongoDatabase db)
    {
        _conversations = db.GetCollection<Conversation>("Conversations");
        _users = db.GetCollection<User>("Users");
        _keys = db.GetCollection<UserKey>("UserKeys");
    }


    // Create Private Conversation
    public async Task<Conversation?> CreatePrivateConversationAsync(
    string userId1, string user1PublicKey, string userId2)
    {
        var user2Key = await _keys.Find(k => k.UserId == userId2).FirstOrDefaultAsync();

        if (user2Key == null)
        {
            throw new Exception("User public key not found");
        }

        var existingConversation = await _conversations.Find(c =>
            c.Type == ConversationType.PRIVATE &&
            c.MemberIds.Contains(userId1) &&
            c.MemberIds.Contains(userId2)
        ).FirstOrDefaultAsync();

        if (existingConversation != null)
        {
            return existingConversation;
        }

        if (userId1 == userId2)
        {
            throw new Exception("Cannot create conversation with oneself.");
        }

        var user1 = await _users.Find(u => u.Id == userId1).FirstOrDefaultAsync();
        var user2 = await _users.Find(u => u.Id == userId2).FirstOrDefaultAsync();

        if (user1 == null || user2 == null)
        {
            throw new Exception("User not found");
        }

        if (user1.BlockedUserIds.Contains(userId2) || user2.BlockedUserIds.Contains(userId1))
        {
            throw new Exception("Cannot create conversation. One of the users has blocked the other.");
        }

        var conversation = new Conversation
        {
            Type = ConversationType.PRIVATE,
            MemberIds = new List<string> { userId1, userId2 },
            MembersPublicKeys = new Dictionary<string, string>
        {
            { userId1, user1PublicKey},
            { userId2, user2Key.PublicKey }
        }
        };

        await _conversations.InsertOneAsync(conversation);

        return conversation;
    }

    // Create Group Conversation
    public async Task<Conversation> CreateGroupAsync(
        string creatorId,
        string groupName,
        List<string> memberIds,
        Dictionary<string, string> membersPublicKeys,
        string? groupAvatarUrl = null
        )
    {
        if (!memberIds.Contains(creatorId))
        {
            memberIds.Add(creatorId);
        }

        var conversation = new Conversation
        {
            Type = ConversationType.GROUP,
            GroupName = groupName,
            MemberIds = memberIds,
            AdminId = creatorId,
            AdminIds = new List<string> { creatorId },
            MembersPublicKeys = membersPublicKeys,
        };

        await _conversations.InsertOneAsync(conversation);

        return conversation;
    }


    public async Task<List<Conversation>> GetUserConversationsAsync(string userId)
    {
        var filter = Builders<Conversation>.Filter.AnyEq(c => c.MemberIds, userId);
        var sort = Builders<Conversation>.Sort.Descending(c => c.LastMessageAt);

        return await _conversations
            .Find(filter)
            .Sort(sort)
            .ToListAsync();
    }


    public async Task UpdateLastMessageAsync(string conversationId, string messageId)
    {
        var update = Builders<Conversation>.Update
            .Set(c => c.LastMessageId, messageId)
            .Set(c => c.LastMessageAt, DateTime.UtcNow)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        await _conversations.UpdateOneAsync(c => c.Id == conversationId, update);
    }

    public async Task<List<ConversationMemberDto>> GetConversationMembersAsync(string conversationId)
    {
        var conversation = await _conversations.Find(c => c.Id == conversationId).FirstOrDefaultAsync();

        if (conversation == null)
        {
            return new List<ConversationMemberDto>();
        }

        var filter = Builders<User>.Filter.In(u => u.Id, conversation.MemberIds);
        var users = await _users.Find(filter).ToListAsync();

        return users.Select(u => new ConversationMemberDto(
            u.Id,
            u.PhoneNumber,
            u.Nickname,
            u.AvatarUrl,
            conversation.MembersPublicKeys.GetValueOrDefault(u.Id, ""),
            u.IsOnline,
            u.LastSeen
        )).ToList();
    }

    public async Task<Conversation?> GetConversationByIdAsync(string conversationId)
    {
        return await _conversations.Find(c => c.Id == conversationId).FirstOrDefaultAsync();
    }

    public async Task AddMmeber(string conversationId, string userId, string publicKey) { 
        var conversation = await GetConversationByIdAsync(conversationId);
        if (conversation == null) return;
        if (!conversation.MemberIds.Contains(userId))
        {
            conversation.MemberIds.Add(userId);
            conversation.MembersPublicKeys[userId] = publicKey;

            var update = Builders<Conversation>.Update
                .Set(c => c.MemberIds, conversation.MemberIds)
                .Set(c => c.MembersPublicKeys, conversation.MembersPublicKeys)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            await _conversations.UpdateOneAsync(c => c.Id == conversationId, update);
        }
    }
}

