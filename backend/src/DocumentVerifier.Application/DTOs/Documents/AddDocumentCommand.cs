namespace DocumentVerifier.Application.DTOs.Documents;

public record AddDocumentCommand(
    string Title,
    string DocumentNumber,
    string DocumentType,
    DateOnly IssueDate,
    Stream FileStream,
    string FileName,
    long FileSize,
    string ContentType,
    Guid IssuerUserId,
    string IssuerName);
