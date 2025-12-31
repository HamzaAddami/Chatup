<<<<<<< HEAD
﻿namespace Chatup.DTOs;
=======
namespace Chatup.DTOs;
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be

public record LoginRequest(string PhoneNumber);

public record VerifyOtpRequest(string PhoneNumber, string Code);

public record AuthResponse(string Token, string UserId, string? Nickname);