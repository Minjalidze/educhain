namespace DocumentVerifier.Application.Interfaces;

public interface IHashingService
{
    Task<string> ComputeSha256HexAsync(Stream stream, CancellationToken cancellationToken);
}
