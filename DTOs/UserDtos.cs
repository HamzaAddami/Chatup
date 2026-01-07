namespace Chatup.DTOs;

public record UpdateProfileRequest(string Nickname, string About, string? AvatarUrl, string? Email);

public record AddContactRequest(string PhoneNumber);

public record BlockUserRequest(string PhoneNumber);

public record UserResponse(string Id, string PhoneNumber, string? Email, string? Nickname, string? About, string? AvatarUrl);