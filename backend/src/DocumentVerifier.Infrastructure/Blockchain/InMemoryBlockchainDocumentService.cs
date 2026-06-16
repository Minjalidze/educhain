using System.Collections.Concurrent;
using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DocumentVerifier.Infrastructure.Blockchain;

public class InMemoryBlockchainDocumentService : IBlockchainDocumentService
{
    private readonly ConcurrentDictionary<string, Entry> _documents = new();
    private readonly BlockchainOptions _options;

    public InMemoryBlockchainDocumentService(IOptions<BlockchainOptions> options)
    {
        _options = options.Value;
    }

    public Task<BlockchainTransactionResult> AddDocumentAsync(string documentHash, CancellationToken cancellationToken)
    {
        _documents.TryAdd(Normalize(documentHash), new Entry("0xmockissuer", DateTimeOffset.UtcNow, false));
        return Task.FromResult(new BlockchainTransactionResult(MockTransactionHash(documentHash), MockContractAddress(), "mock-local"));
    }

    public Task<BlockchainVerificationResult> VerifyDocumentAsync(string documentHash, CancellationToken cancellationToken)
    {
        if (_documents.TryGetValue(Normalize(documentHash), out var entry))
        {
            return Task.FromResult(new BlockchainVerificationResult(true, entry.Revoked, entry.Issuer, entry.IssuedAt));
        }

        return Task.FromResult(new BlockchainVerificationResult(false, false, string.Empty, null));
    }

    public Task<BlockchainTransactionResult> RevokeDocumentAsync(string documentHash, CancellationToken cancellationToken)
    {
        _documents.AddOrUpdate(
            Normalize(documentHash),
            _ => new Entry("0xmockissuer", DateTimeOffset.UtcNow, true),
            (_, current) => current with { Revoked = true });

        return Task.FromResult(new BlockchainTransactionResult(MockTransactionHash(documentHash), MockContractAddress(), "mock-local"));
    }

    private static string Normalize(string documentHash)
    {
        return documentHash.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? documentHash[2..].ToLowerInvariant()
            : documentHash.ToLowerInvariant();
    }

    private string MockContractAddress()
    {
        return string.IsNullOrWhiteSpace(_options.ContractAddress)
            ? "0x0000000000000000000000000000000000000000"
            : _options.ContractAddress;
    }

    private static string MockTransactionHash(string documentHash)
    {
        return "0xmock" + Normalize(documentHash)[..24];
    }

    private record Entry(string Issuer, DateTimeOffset IssuedAt, bool Revoked);
}
