using DocumentVerifier.Application.DTOs.Auth;
using DocumentVerifier.Application.Exceptions;
using DocumentVerifier.Application.Interfaces;

namespace DocumentVerifier.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new AppException("Укажите email и пароль.", 400);
        }

        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException("Неверный email или пароль.", 401);
        }

        return _jwtTokenService.CreateToken(user);
    }
}
