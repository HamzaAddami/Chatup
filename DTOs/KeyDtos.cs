namespace Chatup.DTOs;


public record RegisterPublicKeyRequest(
    string PublicKey  // based 64 encoded
);


public record PublicKeyResponse(
    string UserId,
    string PublicKey    
);