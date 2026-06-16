using DocumentVerifier.Application.DTOs.Auth;

namespace DocumentVerifier.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
