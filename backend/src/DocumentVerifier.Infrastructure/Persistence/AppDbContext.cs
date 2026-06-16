using DocumentVerifier.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerifier.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<EducationalDocument> EducationalDocuments => Set<EducationalDocument>();
    public DbSet<VerificationLog> VerificationLogs => Set<VerificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<EducationalDocument>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.DocumentHash).IsUnique();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DocumentNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DocumentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DocumentHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IssuerName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.BlockchainTransactionHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ContractAddress).HasMaxLength(64).IsRequired();
            entity.Property(x => x.BlockchainNetwork).HasMaxLength(64).IsRequired();

            entity.HasOne(x => x.IssuerUser)
                .WithMany(x => x.IssuedDocuments)
                .HasForeignKey(x => x.IssuerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VerificationLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DocumentHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Result).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.VerifierIp).HasMaxLength(64);
            entity.Property(x => x.BlockchainStatus).HasMaxLength(32).IsRequired();
        });
    }
}
