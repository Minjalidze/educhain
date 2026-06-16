using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Domain.Entities;
using DocumentVerifier.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerifier.Infrastructure.Repositories;

public class VerificationLogRepository : IVerificationLogRepository
{
    private readonly AppDbContext _dbContext;

    public VerificationLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(VerificationLog log, CancellationToken cancellationToken)
    {
        await _dbContext.VerificationLogs.AddAsync(log, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return _dbContext.VerificationLogs.CountAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
