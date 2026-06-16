namespace DocumentVerifier.Application.DTOs.Documents;

public record VerifyDocumentCommand(
    Stream FileStream,
    string FileName,
    long FileSize,
    string ContentType,
    string? VerifierIp,
    Guid? VerifierUserId);
