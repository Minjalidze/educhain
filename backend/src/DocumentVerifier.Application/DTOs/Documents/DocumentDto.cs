namespace DocumentVerifier.Application.DTOs.Documents;

public record DocumentDto(
    Guid Id,
    string Title,
    string DocumentNumber,
    string DocumentType,
    string DocumentHash,
    string IssuerName,
    DateOnly IssueDate,
    string BlockchainTransactionHash,
    string ContractAddress,
    string BlockchainNetwork,
    bool IsRevoked,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt);
