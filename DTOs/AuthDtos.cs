namespace Chatup.DTOs;

public record LoginRequest(string PhoneNumber);

public record VerifyOtpRequest(string PhoneNumber, string Code);

public record AuthResponse(string Token, string UserId, string? Nickname, string? Email);

public record FirebaseLoginRequest(string FirebaseToken);
