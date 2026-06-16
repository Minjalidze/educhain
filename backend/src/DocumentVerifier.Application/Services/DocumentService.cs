using DocumentVerifier.Application.DTOs.Documents;
using DocumentVerifier.Application.Exceptions;
using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Domain.Entities;
using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.Services;

public class DocumentService : IDocumentService
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "text/plain",
        "application/octet-stream"
    };

    private readonly IEducationalDocumentRepository _documents;
    private readonly IVerificationLogRepository _verificationLogs;
    private readonly IHashingService _hashingService;
    private readonly IBlockchainDocumentService _blockchain;

    public DocumentService(
        IEducationalDocumentRepository documents,
        IVerificationLogRepository verificationLogs,
        IHashingService hashingService,
        IBlockchainDocumentService blockchain)
    {
        _documents = documents;
        _verificationLogs = verificationLogs;
        _hashingService = hashingService;
        _blockchain = blockchain;
    }

    public async Task<AddDocumentResult> AddDocumentAsync(AddDocumentCommand command, CancellationToken cancellationToken)
    {
        ValidateMetadata(command.Title, command.DocumentNumber, command.DocumentType);
        ValidateFile(command.FileName, command.FileSize, command.ContentType);

        var documentHash = await _hashingService.ComputeSha256HexAsync(command.FileStream, cancellationToken);

        if (await _documents.GetByHashAsync(documentHash, cancellationToken) is not null)
        {
            throw new AppException("Документ с таким хэшем уже зарегистрирован.", 409);
        }

        var transaction = await _blockchain.AddDocumentAsync(documentHash, cancellationToken);

        var document = new EducationalDocument
        {
            Title = command.Title.Trim(),
            DocumentNumber = command.DocumentNumber.Trim(),
            DocumentType = command.DocumentType.Trim(),
            DocumentHash = documentHash,
            IssuerUserId = command.IssuerUserId,
            IssuerName = command.IssuerName.Trim(),
            IssueDate = command.IssueDate,
            BlockchainTransactionHash = transaction.TransactionHash,
            ContractAddress = transaction.ContractAddress,
            BlockchainNetwork = transaction.Network
        };

        await _documents.AddAsync(document, cancellationToken);
        await _documents.SaveChangesAsync(cancellationToken);

        var dto = Map(document);
        return new AddDocumentResult(dto, documentHash, transaction.TransactionHash, transaction.ContractAddress, transaction.Network);
    }

    public async Task<VerifyDocumentResult> VerifyDocumentAsync(VerifyDocumentCommand command, CancellationToken cancellationToken)
    {
        ValidateFile(command.FileName, command.FileSize, command.ContentType);

        var documentHash = await _hashingService.ComputeSha256HexAsync(command.FileStream, cancellationToken);
        var blockchainStatus = await _blockchain.VerifyDocumentAsync(documentHash, cancellationToken);
        var document = await _documents.GetByHashAsync(documentHash, cancellationToken);

        VerificationResultStatus status;
        string message;

        if (!blockchainStatus.Exists)
        {
            status = VerificationResultStatus.NotFound;
            message = "Документ не найден в блокчейне или был изменён.";
        }
        else if (blockchainStatus.Revoked || document?.IsRevoked == true)
        {
            status = VerificationResultStatus.Revoked;
            message = "Документ найден, но был отозван.";
        }
        else
        {
            status = VerificationResultStatus.Valid;
            message = "Документ найден и является действительным.";
        }

        await _verificationLogs.AddAsync(new VerificationLog
        {
            DocumentHash = documentHash,
            Result = status,
            VerifierIp = command.VerifierIp,
            VerifierUserId = command.VerifierUserId,
            BlockchainStatus = blockchainStatus.Exists ? (blockchainStatus.Revoked ? "revoked" : "active") : "not_found"
        }, cancellationToken);
        await _verificationLogs.SaveChangesAsync(cancellationToken);

        return new VerifyDocumentResult(
            status,
            message,
            documentHash,
            blockchainStatus.Exists ? (blockchainStatus.Revoked ? "revoked" : "active") : "not_found",
            document is null ? null : Map(document));
    }

    public async Task<IReadOnlyList<DocumentDto>> ListDocumentsAsync(
        Guid? requesterUserId,
        UserRole requesterRole,
        CancellationToken cancellationToken)
    {
        var documents = await _documents.ListAsync(requesterUserId, requesterRole, cancellationToken);
        return documents.Select(Map).ToArray();
    }

    public async Task<DocumentDto> RevokeDocumentAsync(
        Guid documentId,
        Guid requesterUserId,
        UserRole requesterRole,
        CancellationToken cancellationToken)
    {
        var document = await _documents.GetByIdAsync(documentId, cancellationToken)
            ?? throw new AppException("Документ не найден.", 404);

        if (requesterRole != UserRole.Admin && document.IssuerUserId != requesterUserId)
        {
            throw new AppException("Можно отзывать только документы своей организации.", 403);
        }

        if (document.IsRevoked)
        {
            return Map(document);
        }

        await _blockchain.RevokeDocumentAsync(document.DocumentHash, cancellationToken);

        document.IsRevoked = true;
        document.RevokedAt = DateTimeOffset.UtcNow;
        document.UpdatedAt = DateTimeOffset.UtcNow;

        await _documents.SaveChangesAsync(cancellationToken);
        return Map(document);
    }

    public async Task<DashboardStatsDto> GetDashboardAsync(
        Guid? requesterUserId,
        UserRole requesterRole,
        CancellationToken cancellationToken)
    {
        var total = await _documents.CountAsync(requesterUserId, requesterRole, cancellationToken);
        var revoked = await _documents.CountRevokedAsync(requesterUserId, requesterRole, cancellationToken);
        var recent = await _documents.RecentAsync(requesterUserId, requesterRole, 5, cancellationToken);
        var checks = await _verificationLogs.CountAsync(cancellationToken);

        return new DashboardStatsDto(total, revoked, total - revoked, checks, recent.Select(Map).ToArray());
    }

    private static void ValidateMetadata(string title, string documentNumber, string documentType)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(documentNumber) || string.IsNullOrWhiteSpace(documentType))
        {
            throw new AppException("Название, номер и тип документа обязательны.", 400);
        }
    }

    private static void ValidateFile(string fileName, long fileSize, string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileSize <= 0)
        {
            throw new AppException("Файл документа обязателен.", 400);
        }

        if (fileSize > MaxFileSize)
        {
            throw new AppException("Размер файла не должен превышать 10 МБ.", 400);
        }

        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new AppException("Поддерживаются PDF, PNG, JPG и TXT файлы.", 400);
        }
    }

    private static DocumentDto Map(EducationalDocument document)
    {
        return new DocumentDto(
            document.Id,
            document.Title,
            document.DocumentNumber,
            document.DocumentType,
            document.DocumentHash,
            document.IssuerName,
            document.IssueDate,
            document.BlockchainTransactionHash,
            document.ContractAddress,
            document.BlockchainNetwork,
            document.IsRevoked,
            document.RevokedAt,
            document.CreatedAt);
    }
}
