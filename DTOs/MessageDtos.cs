namespace Chatup.DTOs;

public record SendMessageRequest(
    string ConversationId,
    string CipherText,
    string Iv
);

public record MessageResponse(
    string Id,
    string ConversationId,
    string SenderId,
    string Nickname,
    string? AvatarUrl,
    string CipherText,
    string Iv,
    string Status,
    List<string> ReadBy,
    DateTime CreatedAt
);

public record MessageNotification(
    MessageResponse Message,
    string ConversationId
);

public record MarkMessagesReadRequest(
    string ConversationId,
    List<string> MessageIds
);