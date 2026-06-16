using DocumentVerifier.Application.DTOs.Users;

namespace DocumentVerifier.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken cancellationToken);
}
