using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chatup.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;
    
    public string? Nickname { get; set; }
    public string? About { get; set; }
    public string? AvatarUrl { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> ContactIds { get; set; } = new();
    
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> BlockedUserIds { get; set; } = new();

    public bool IsOnline { get; set; } = false;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}