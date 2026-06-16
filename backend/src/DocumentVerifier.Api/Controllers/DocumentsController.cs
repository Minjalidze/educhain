using System.Security.Claims;
using DocumentVerifier.Api.Models;
using DocumentVerifier.Application.DTOs.Documents;
using DocumentVerifier.Application.Exceptions;
using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentVerifier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Issuer")]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _documentService.ListDocumentsAsync(CurrentUserId(), CurrentRole(), cancellationToken));
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,Issuer")]
    public async Task<ActionResult<DashboardStatsDto>> Dashboard(CancellationToken cancellationToken)
    {
        return Ok(await _documentService.GetDashboardAsync(CurrentUserId(), CurrentRole(), cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Issuer")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<AddDocumentResult>> Add([FromForm] AddDocumentForm form, CancellationToken cancellationToken)
    {
        if (form.File is null)
        {
            throw new AppException("Файл документа обязателен.", 400);
        }

        await using var stream = form.File.OpenReadStream();
        var result = await _documentService.AddDocumentAsync(
            new AddDocumentCommand(
                form.Title,
                form.DocumentNumber,
                form.DocumentType,
                form.IssueDate,
                stream,
                form.File.FileName,
                form.File.Length,
                NormalizeContentType(form.File.ContentType),
                CurrentUserId(),
                User.FindFirstValue(ClaimTypes.Name) ?? "Issuer"),
            cancellationToken);

        return CreatedAtAction(nameof(List), new { id = result.Document.Id }, result);
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<VerifyDocumentResult>> Verify([FromForm] VerifyDocumentForm form, CancellationToken cancellationToken)
    {
        if (form.File is null)
        {
            throw new AppException("Файл документа обязателен.", 400);
        }

        await using var stream = form.File.OpenReadStream();
        var result = await _documentService.VerifyDocumentAsync(
            new VerifyDocumentCommand(
                stream,
                form.File.FileName,
                form.File.Length,
                NormalizeContentType(form.File.ContentType),
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                TryCurrentUserId()),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/revoke")]
    [Authorize(Roles = "Admin,Issuer")]
    public async Task<ActionResult<DocumentDto>> Revoke(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _documentService.RevokeDocumentAsync(id, CurrentUserId(), CurrentRole(), cancellationToken));
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new AppException("Не удалось определить пользователя.", 401);
    }

    private Guid? TryCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private UserRole CurrentRole()
    {
        var value = User.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<UserRole>(value, out var role)
            ? role
            : throw new AppException("Не удалось определить роль пользователя.", 401);
    }

    private static string NormalizeContentType(string? contentType)
    {
        return string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
    }
}
