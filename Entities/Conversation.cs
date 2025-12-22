using Chatup.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chatup.Entities;

public class Conversation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id = null! ;

    public ConversationType Type { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? LastMessageId { get; set; }
    public List<string> MemberIds { get; set; } = new();

    [BsonIgnoreIfNull]
    public string? GroupName { get; set; } = null!;

    [BsonIgnoreIfNull]
    public string? GroupAvatarUrl { get; set; } = null!;


    [BsonRepresentation(BsonType.ObjectId)]
    public string? AdminId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> AdminIds { get; set; } = null!;


    public Dictionary<string, string> MembersPublicKeys { get; set; } = new();


    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}

