<<<<<<< HEAD
﻿using MongoDB.Bson;
=======
using MongoDB.Bson;
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
using MongoDB.Bson.Serialization.Attributes;

namespace Chatup.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;
<<<<<<< HEAD

    public string? Nickname { get; set; }
    public string? About { get; set; }
    public string? AvatarUrl { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> ContactIds { get; set; } = new();

=======
    
    public string? Nickname { get; set; }
    public string? About { get; set; }
    public string? AvatarUrl { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> ContactIds { get; set; } = new();
    
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> BlockedUserIds { get; set; } = new();

    public bool IsOnline { get; set; } = false;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
<<<<<<< HEAD

=======
    
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}