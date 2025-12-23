using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Chatup.Entities;

public class UserKey
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;
    public string PublicKey { get; set; } = null!;


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

