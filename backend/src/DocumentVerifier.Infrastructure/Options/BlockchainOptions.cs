namespace DocumentVerifier.Infrastructure.Options;

public class BlockchainOptions
{
    public bool UseMock { get; set; }
    public string RpcUrl { get; set; } = "http://127.0.0.1:8545";
    public long ChainId { get; set; } = 31337;
    public string ContractAddress { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string NetworkName { get; set; } = "hardhat-local";
}
