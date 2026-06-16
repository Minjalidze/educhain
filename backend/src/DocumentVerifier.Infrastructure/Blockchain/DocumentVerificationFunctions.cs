using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace DocumentVerifier.Infrastructure.Blockchain;

[Function("addDocument")]
public class AddDocumentFunction : FunctionMessage
{
    [Parameter("bytes32", "documentHash", 1)]
    public byte[] DocumentHash { get; set; } = Array.Empty<byte>();
}

[Function("verifyDocument", typeof(VerifyDocumentOutputDto))]
public class VerifyDocumentFunction : FunctionMessage
{
    [Parameter("bytes32", "documentHash", 1)]
    public byte[] DocumentHash { get; set; } = Array.Empty<byte>();
}

[FunctionOutput]
public class VerifyDocumentOutputDto : IFunctionOutputDTO
{
    [Parameter("bool", "exists", 1)]
    public bool Exists { get; set; }

    [Parameter("bool", "revoked", 2)]
    public bool Revoked { get; set; }

    [Parameter("address", "issuer", 3)]
    public string Issuer { get; set; } = string.Empty;

    [Parameter("uint256", "issuedAt", 4)]
    public BigInteger IssuedAt { get; set; }
}

[Function("revokeDocument")]
public class RevokeDocumentFunction : FunctionMessage
{
    [Parameter("bytes32", "documentHash", 1)]
    public byte[] DocumentHash { get; set; } = Array.Empty<byte>();
}
