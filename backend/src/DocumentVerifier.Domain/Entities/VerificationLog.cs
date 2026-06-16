using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Domain.Entities;

public class VerificationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DocumentHash { get; set; } = string.Empty;
    public VerificationResultStatus Result { get; set; }
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? VerifierIp { get; set; }
    public Guid? VerifierUserId { get; set; }
    public string BlockchainStatus { get; set; } = string.Empty;
}
