using System.Text;
using DocumentVerifier.Infrastructure.Security;

namespace DocumentVerifier.UnitTests;

public class SecurityTests
{
    [Fact]
    public async Task HashingService_ComputesExpectedSha256()
    {
        var service = new HashingService();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("document"));

        var hash = await service.ComputeSha256HexAsync(stream, CancellationToken.None);

        Assert.Equal("43cc23fa52b87b4cc1d02b5b114154151d6adddb17c9fddc06b027fa99e24008", hash);
    }

    [Fact]
    public void PasswordHasher_VerifiesOriginalPassword()
    {
        var service = new PasswordHasher();

        var hash = service.Hash("issuer123");

        Assert.True(service.Verify("issuer123", hash));
        Assert.False(service.Verify("wrong-password", hash));
    }
}
