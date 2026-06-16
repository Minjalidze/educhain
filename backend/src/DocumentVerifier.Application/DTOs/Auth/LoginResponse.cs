namespace DocumentVerifier.Application.DTOs.Auth;

public record LoginResponse(string Token, DateTimeOffset ExpiresAt, AuthUserDto User);
