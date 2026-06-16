namespace DocumentVerifier.Api.Models;

public class AddDocumentForm
{
    public string Title { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public IFormFile? File { get; set; }
}
