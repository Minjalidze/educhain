using DocumentVerifier.Application.DTOs.Documents;
using DocumentVerifier.Domain.Enums;

namespace DocumentVerifier.Application.Interfaces;

public interface IDocumentService
{
    Task<AddDocumentResult> AddDocumentAsync(AddDocumentCommand command, CancellationToken cancellationToken);
    Task<VerifyDocumentResult> VerifyDocumentAsync(VerifyDocumentCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentDto>> ListDocumentsAsync(Guid? requesterUserId, UserRole requesterRole, CancellationToken cancellationToken);
    Task<DocumentDto> RevokeDocumentAsync(Guid documentId, Guid requesterUserId, UserRole requesterRole, CancellationToken cancellationToken);
    Task<DashboardStatsDto> GetDashboardAsync(Guid? requesterUserId, UserRole requesterRole, CancellationToken cancellationToken);
}
