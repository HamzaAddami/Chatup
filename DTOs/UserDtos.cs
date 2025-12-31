<<<<<<< HEAD
﻿namespace Chatup.DTOs;
=======
namespace Chatup.DTOs;
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be

public record UpdateProfileRequest(string Nickname, string About, string? AvatarUrl);

public record AddContactRequest(string PhoneNumber);

public record BlockUserRequest(string PhoneNumber);

<<<<<<< HEAD
public record UserResponse(string Id, string PhoneNumber, string? Nickname, string? About, string? AvatarUrl);
=======
public record UserResponse(string Id, string PhoneNumber, string? Nickname, string? About, string? AvatarUrl);

>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
