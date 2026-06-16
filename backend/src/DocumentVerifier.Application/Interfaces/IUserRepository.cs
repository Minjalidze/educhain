using DocumentVerifier.Domain.Entities;
using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, Guid exceptUserId, CancellationToken cancellationToken);
    Task<int> CountByRoleAsync(UserRole role, CancellationToken cancellationToken);
    Task<bool> HasIssuedDocumentsAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    void Remove(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
