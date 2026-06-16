using System.Security.Cryptography;
using DocumentVerifier.Application.Interfaces;

namespace DocumentVerifier.Infrastructure.Security;

public class HashingService : IHashingService
{
    public async Task<string> ComputeSha256HexAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
