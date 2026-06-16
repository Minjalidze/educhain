using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.DTOs.Users;

public record UpdateUserRequest(string Email, string FullName, UserRole Role, string? Password);
