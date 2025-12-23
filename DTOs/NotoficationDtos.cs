namespace Chatup.DTOs;


public record MessageStatusNotification(
    string MessageId,
    string ConversationId,
    string Status,
    List<string> ReadBy
);

public record TypingNotification(
    string ConversationId,
    string UserId,
    string NickName,
    bool IsTyping
);




