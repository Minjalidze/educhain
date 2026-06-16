using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Domain.Entities;
using DocumentVerifier.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentVerifier.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            "admin@example.local",
            "Администратор системы",
            UserRole.Admin,
            "admin123",
            cancellationToken);

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            "issuer@example.local",
            "НКЭИВТ",
            UserRole.Issuer,
            "issuer123",
            cancellationToken);

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            "verifier@example.local",
            "Проверяющий пользователь",
            UserRole.Verifier,
            "verifier123",
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureUserAsync(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        string email,
        string fullName,
        UserRole role,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (exists)
        {
            return;
        }

        await dbContext.Users.AddAsync(new User
        {
            Email = normalizedEmail,
            FullName = fullName,
            Role = role,
            PasswordHash = passwordHasher.Hash(password)
        }, cancellationToken);
    }
}
