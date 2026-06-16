using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.DTOs.Documents;

public record VerifyDocumentResult(
    VerificationResultStatus Status,
    string Message,
    string DocumentHash,
    string BlockchainStatus,
    DocumentDto? Document);
