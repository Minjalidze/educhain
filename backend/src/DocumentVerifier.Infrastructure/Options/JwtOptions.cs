namespace DocumentVerifier.Infrastructure.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = "DocumentVerifier";
    public string Audience { get; set; } = "DocumentVerifier.Web";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 120;
}
