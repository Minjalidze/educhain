using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentVerifier.Application.DTOs.Auth;
using DocumentVerifier.Application.DTOs.Documents;

namespace DocumentVerifier.IntegrationTests;

public class DocumentsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public DocumentsFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MainDocumentScenario_WorksWithMockBlockchain()
    {
        var token = await LoginAsIssuer();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var addResponse = await _client.PostAsync("/api/documents", CreateDocumentContent("demo diploma"));
        addResponse.EnsureSuccessStatusCode();
        var added = await addResponse.Content.ReadFromJsonAsync<AddDocumentResult>(_jsonOptions);
        Assert.NotNull(added);
        Assert.Equal("mock-local", added!.BlockchainNetwork);

        var verifyResponse = await _client.PostAsync("/api/documents/verify", CreateVerifyContent("demo diploma"));
        verifyResponse.EnsureSuccessStatusCode();
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<VerifyDocumentResult>(_jsonOptions);
        Assert.Equal("Valid", verifyResult!.Status.ToString());

        var changedResponse = await _client.PostAsync("/api/documents/verify", CreateVerifyContent("changed diploma"));
        changedResponse.EnsureSuccessStatusCode();
        var changedResult = await changedResponse.Content.ReadFromJsonAsync<VerifyDocumentResult>(_jsonOptions);
        Assert.Equal("NotFound", changedResult!.Status.ToString());

        var revokeResponse = await _client.PostAsync($"/api/documents/{added.Document.Id}/revoke", null);
        revokeResponse.EnsureSuccessStatusCode();

        var revokedResponse = await _client.PostAsync("/api/documents/verify", CreateVerifyContent("demo diploma"));
        revokedResponse.EnsureSuccessStatusCode();
        var revokedResult = await revokedResponse.Content.ReadFromJsonAsync<VerifyDocumentResult>(_jsonOptions);
        Assert.Equal("Revoked", revokedResult!.Status.ToString());
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<string> LoginAsIssuer()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("issuer@example.local", "issuer123"));
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
        return result!.Token;
    }

    private static MultipartFormDataContent CreateDocumentContent(string content)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent("Диплом о среднем профессиональном образовании"), "Title" },
            { new StringContent("NKEIVT-2026-001"), "DocumentNumber" },
            { new StringContent("Diploma"), "DocumentType" },
            { new StringContent("2026-05-18"), "IssueDate" }
        };

        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        form.Add(file, "File", "diploma.txt");
        return form;
    }

    private static MultipartFormDataContent CreateVerifyContent(string content)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        form.Add(file, "File", "diploma.txt");
        return form;
    }
}
