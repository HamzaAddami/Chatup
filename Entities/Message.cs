using Chatup.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chatup.Entities;
public class Message
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id = null!;


    [BsonRepresentation(BsonType.ObjectId)]
    public string ConversationId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string SenderId { get; set; } = null!;

    public string Ciphertext { get; set; } = null!;

    public string IV { get; set; } = null!;

    public MessageStatus Status { get; set; } = MessageStatus.SENT;

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> ReadBy { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;






}

