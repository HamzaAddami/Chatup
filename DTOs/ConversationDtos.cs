using Chatup.Entities;

namespace Chatup.DTOs;

public record CreatePrivateConversationRequest(
    string ContactUserId,  
    string MyPublicKey
);

public record CreateGroupRequest(
    string GroupName, 
    List<string> MemberIds, 
    Dictionary<string, string> MembersPublicKeys, 
    string? GroupAvatarUrl
);

public record ConversationMemberDto(
    string UserId,
    string PhoneNumber,
    string? Nickname,
    string? AvatarUrl,
    string PublicKey,
    bool IsOnline,
    DateTime LastSeen
);

public record ConversationResponse(
    string Id,
    string Type,
    string? GroupName,
    string? GroupAvatarUrl,
    List<ConversationMemberDto> Members,
    MessageResponse? LastMessage,
    DateTime LastMessageAt,
    int UnreadCount
);

