using DocumentVerifier.Application.DTOs.Auth;
using DocumentVerifier.Domain.Entities;

namespace DocumentVerifier.Application.Interfaces;

public interface IJwtTokenService
{
    LoginResponse CreateToken(User user);
}
