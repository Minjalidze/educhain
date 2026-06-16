using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Application.Services;
using DocumentVerifier.Infrastructure.Blockchain;
using DocumentVerifier.Infrastructure.Options;
using DocumentVerifier.Infrastructure.Persistence;
using DocumentVerifier.Infrastructure.Repositories;
using DocumentVerifier.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentVerifier.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentVerifierInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<BlockchainOptions>(configuration.GetSection("Blockchain"));

        if (string.Equals(configuration["Database:Provider"], "InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(configuration["Database:InMemoryName"] ?? "document-verifier-tests"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        }

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDocumentService, DocumentService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEducationalDocumentRepository, EducationalDocumentRepository>();
        services.AddScoped<IVerificationLogRepository, VerificationLogRepository>();

        services.AddScoped<IHashingService, HashingService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        if (configuration.GetValue<bool>("Blockchain:UseMock"))
        {
            services.AddSingleton<IBlockchainDocumentService, InMemoryBlockchainDocumentService>();
        }
        else
        {
            services.AddScoped<IBlockchainDocumentService, EthereumDocumentRegistryService>();
        }

        return services;
    }
}
