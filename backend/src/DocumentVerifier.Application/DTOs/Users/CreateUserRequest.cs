using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.DTOs.Users;

public record CreateUserRequest(string Email, string Password, string FullName, UserRole Role);
