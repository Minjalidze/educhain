using DocumentVerifier.Domain.Entities;

namespace DocumentVerifier.Application.Interfaces;

public interface IVerificationLogRepository
{
    Task AddAsync(VerificationLog log, CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
