using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Domain.Entities;
using DocumentVerifier.Domain.Enums;
using DocumentVerifier.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerifier.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users.OrderBy(x => x.Email).ToArrayAsync(cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, Guid exceptUserId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AnyAsync(x => x.Email == email && x.Id != exceptUserId, cancellationToken);
    }

    public Task<int> CountByRoleAsync(UserRole role, CancellationToken cancellationToken)
    {
        return _dbContext.Users.CountAsync(x => x.Role == role, cancellationToken);
    }

    public Task<bool> HasIssuedDocumentsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.EducationalDocuments.AnyAsync(x => x.IssuerUserId == userId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public void Remove(User user)
    {
        _dbContext.Users.Remove(user);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
