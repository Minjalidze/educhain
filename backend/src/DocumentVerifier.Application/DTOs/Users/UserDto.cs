using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.DTOs.Users;

public record UserDto(Guid Id, string Email, string FullName, UserRole Role, DateTimeOffset CreatedAt);
