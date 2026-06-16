using DocumentVerifier.Application.DTOs.Users;
using DocumentVerifier.Application.Exceptions;
using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Domain.Entities;
using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new AppException("Email, пароль и имя обязательны.", 400);
        }

        if (request.Password.Length < 6)
        {
            throw new AppException("Пароль должен быть не короче 6 символов.", 400);
        }

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new AppException("Пользователь с таким email уже существует.", 409);
        }

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            Role = request.Role,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        await _users.AddAsync(user, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    public async Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken cancellationToken)
    {
        var users = await _users.ListAsync(cancellationToken);
        return users.Select(Map).ToArray();
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Пользователь не найден.", 404);

        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new AppException("Email и имя обязательны.", 400);
        }

        if (!string.IsNullOrWhiteSpace(request.Password) && request.Password.Length < 6)
        {
            throw new AppException("Пароль должен быть не короче 6 символов.", 400);
        }

        if (await _users.ExistsByEmailAsync(email, user.Id, cancellationToken))
        {
            throw new AppException("Пользователь с таким email уже существует.", 409);
        }

        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin)
        {
            var admins = await _users.CountByRoleAsync(UserRole.Admin, cancellationToken);
            if (admins <= 1)
            {
                throw new AppException("Нельзя снять роль у последнего администратора.", 400);
            }
        }

        user.Email = email;
        user.FullName = request.FullName.Trim();
        user.Role = request.Role;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        await _users.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Пользователь не найден.", 404);

        if (user.Role == UserRole.Admin)
        {
            var admins = await _users.CountByRoleAsync(UserRole.Admin, cancellationToken);
            if (admins <= 1)
            {
                throw new AppException("Нельзя удалить последнего администратора.", 400);
            }
        }

        if (await _users.HasIssuedDocumentsAsync(user.Id, cancellationToken))
        {
            throw new AppException("Нельзя удалить пользователя, у которого есть зарегистрированные документы.", 400);
        }

        _users.Remove(user);
        await _users.SaveChangesAsync(cancellationToken);
    }

    private static UserDto Map(User user)
    {
        return new UserDto(user.Id, user.Email, user.FullName, user.Role, user.CreatedAt);
    }
}
