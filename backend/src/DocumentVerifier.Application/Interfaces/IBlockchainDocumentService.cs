namespace DocumentVerifier.Application.Interfaces;

public interface IBlockchainDocumentService
{
    Task<BlockchainTransactionResult> AddDocumentAsync(string documentHash, CancellationToken cancellationToken);
    Task<BlockchainVerificationResult> VerifyDocumentAsync(string documentHash, CancellationToken cancellationToken);
    Task<BlockchainTransactionResult> RevokeDocumentAsync(string documentHash, CancellationToken cancellationToken);
}

public record BlockchainTransactionResult(string TransactionHash, string ContractAddress, string Network);

public record BlockchainVerificationResult(
    bool Exists,
    bool Revoked,
    string IssuerAddress,
    DateTimeOffset? IssuedAt);
