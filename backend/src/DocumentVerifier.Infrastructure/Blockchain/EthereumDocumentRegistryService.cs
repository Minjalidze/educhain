using System.Numerics;
using DocumentVerifier.Application.Exceptions;
using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace DocumentVerifier.Infrastructure.Blockchain;

public class EthereumDocumentRegistryService : IBlockchainDocumentService
{
    private readonly BlockchainOptions _options;

    public EthereumDocumentRegistryService(IOptions<BlockchainOptions> options)
    {
        _options = options.Value;
    }

    public async Task<BlockchainTransactionResult> AddDocumentAsync(string documentHash, CancellationToken cancellationToken)
    {
        var web3 = CreateWriteWeb3();
        var handler = web3.Eth.GetContractTransactionHandler<AddDocumentFunction>();
        var function = new AddDocumentFunction
        {
            DocumentHash = ParseBytes32(documentHash),
            Gas = new HexBigInteger(900_000)
        };

        var txHash = await handler.SendRequestAsync(_options.ContractAddress, function).WaitAsync(cancellationToken);
        return new BlockchainTransactionResult(txHash, _options.ContractAddress, _options.NetworkName);
    }

    public async Task<BlockchainVerificationResult> VerifyDocumentAsync(string documentHash, CancellationToken cancellationToken)
    {
        EnsureContractConfigured();

        var web3 = new Web3(_options.RpcUrl);
        var handler = web3.Eth.GetContractQueryHandler<VerifyDocumentFunction>();
        var output = await handler
            .QueryDeserializingToObjectAsync<VerifyDocumentOutputDto>(
                new VerifyDocumentFunction { DocumentHash = ParseBytes32(documentHash) },
                _options.ContractAddress)
            .WaitAsync(cancellationToken);

        return new BlockchainVerificationResult(
            output.Exists,
            output.Revoked,
            output.Issuer,
            output.IssuedAt > BigInteger.Zero ? DateTimeOffset.FromUnixTimeSeconds((long)output.IssuedAt) : null);
    }

    public async Task<BlockchainTransactionResult> RevokeDocumentAsync(string documentHash, CancellationToken cancellationToken)
    {
        var web3 = CreateWriteWeb3();
        var handler = web3.Eth.GetContractTransactionHandler<RevokeDocumentFunction>();
        var function = new RevokeDocumentFunction
        {
            DocumentHash = ParseBytes32(documentHash),
            Gas = new HexBigInteger(900_000)
        };

        var txHash = await handler.SendRequestAsync(_options.ContractAddress, function).WaitAsync(cancellationToken);
        return new BlockchainTransactionResult(txHash, _options.ContractAddress, _options.NetworkName);
    }

    private Web3 CreateWriteWeb3()
    {
        EnsureContractConfigured();

        if (string.IsNullOrWhiteSpace(_options.PrivateKey))
        {
            throw new AppException("Не задан приватный ключ Ethereum-аккаунта для записи в контракт.", 500);
        }

        var account = new Account(_options.PrivateKey, _options.ChainId);
        return new Web3(account, _options.RpcUrl);
    }

    private void EnsureContractConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.RpcUrl) || string.IsNullOrWhiteSpace(_options.ContractAddress))
        {
            throw new AppException("Не настроены RPC URL или адрес смарт-контракта.", 500);
        }
    }

    private static byte[] ParseBytes32(string documentHash)
    {
        var normalized = documentHash.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? documentHash[2..]
            : documentHash;

        if (normalized.Length != 64)
        {
            throw new AppException("Хэш документа должен быть SHA-256 в hex-формате.", 400);
        }

        return Convert.FromHexString(normalized);
    }
}
