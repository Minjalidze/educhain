namespace DocumentVerifier.Application.DTOs.Documents;

public record DashboardStatsDto(
    int TotalDocuments,
    int RevokedDocuments,
    int ActiveDocuments,
    int VerificationChecks,
    IReadOnlyList<DocumentDto> RecentDocuments);
