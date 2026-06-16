using DocumentVerifier.Domain.Entities;
using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.Interfaces;

public interface IEducationalDocumentRepository
{
    Task AddAsync(EducationalDocument document, CancellationToken cancellationToken);
    Task<EducationalDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<EducationalDocument?> GetByHashAsync(string documentHash, CancellationToken cancellationToken);
    Task<IReadOnlyList<EducationalDocument>> ListAsync(Guid? issuerUserId, UserRole requesterRole, CancellationToken cancellationToken);
    Task<IReadOnlyList<EducationalDocument>> RecentAsync(Guid? issuerUserId, UserRole requesterRole, int take, CancellationToken cancellationToken);
    Task<int> CountAsync(Guid? issuerUserId, UserRole requesterRole, CancellationToken cancellationToken);
    Task<int> CountRevokedAsync(Guid? issuerUserId, UserRole requesterRole, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
