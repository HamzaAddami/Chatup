namespace Chatup.DTOs;

public record UpdateProfileRequest(string Nickname, string About, string? AvatarUrl);

public record AddContactRequest(string PhoneNumber);

public record BlockUserRequest(string PhoneNumber);

public record UserResponse(string Id, string PhoneNumber, string? Nickname, string? About, string? AvatarUrl);

