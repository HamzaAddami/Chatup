using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Chatup.Entities;

public class UserKeys
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public int Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    public string PublicKey { get; set; } = null!;

}

