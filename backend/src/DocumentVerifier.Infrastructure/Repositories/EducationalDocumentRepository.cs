using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Domain.Entities;
using DocumentVerifier.Domain.Enums;
using DocumentVerifier.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerifier.Infrastructure.Repositories;

public class EducationalDocumentRepository : IEducationalDocumentRepository
{
    private readonly AppDbContext _dbContext;

    public EducationalDocumentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EducationalDocument document, CancellationToken cancellationToken)
    {
        await _dbContext.EducationalDocuments.AddAsync(document, cancellationToken);
    }

    public Task<EducationalDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.EducationalDocuments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<EducationalDocument?> GetByHashAsync(string documentHash, CancellationToken cancellationToken)
    {
        return _dbContext.EducationalDocuments.FirstOrDefaultAsync(x => x.DocumentHash == documentHash, cancellationToken);
    }

    public async Task<IReadOnlyList<EducationalDocument>> ListAsync(
        Guid? issuerUserId,
        UserRole requesterRole,
        CancellationToken cancellationToken)
    {
        return await Scope(issuerUserId, requesterRole)
            .OrderByDescending(x => x.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EducationalDocument>> RecentAsync(
        Guid? issuerUserId,
        UserRole requesterRole,
        int take,
        CancellationToken cancellationToken)
    {
        return await Scope(issuerUserId, requesterRole)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid? issuerUserId, UserRole requesterRole, CancellationToken cancellationToken)
    {
        return Scope(issuerUserId, requesterRole).CountAsync(cancellationToken);
    }

    public Task<int> CountRevokedAsync(Guid? issuerUserId, UserRole requesterRole, CancellationToken cancellationToken)
    {
        return Scope(issuerUserId, requesterRole).CountAsync(x => x.IsRevoked, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<EducationalDocument> Scope(Guid? issuerUserId, UserRole requesterRole)
    {
        var query = _dbContext.EducationalDocuments.AsQueryable();

        if (requesterRole == UserRole.Issuer && issuerUserId.HasValue)
        {
            query = query.Where(x => x.IssuerUserId == issuerUserId.Value);
        }

        return query;
    }
}
