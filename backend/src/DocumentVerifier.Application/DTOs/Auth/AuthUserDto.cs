using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.DTOs.Auth;

public record AuthUserDto(Guid Id, string Email, string FullName, UserRole Role);
