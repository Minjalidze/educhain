namespace DocumentVerifier.Application.DTOs.Documents;

public record AddDocumentResult(
    DocumentDto Document,
    string DocumentHash,
    string TransactionHash,
    string ContractAddress,
    string BlockchainNetwork);
