namespace DocumentVerifier.Domain.Entities;

public class EducationalDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentHash { get; set; } = string.Empty;
    public Guid IssuerUserId { get; set; }
    public string IssuerName { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public string BlockchainTransactionHash { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public string BlockchainNetwork { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? IssuerUser { get; set; }
}
