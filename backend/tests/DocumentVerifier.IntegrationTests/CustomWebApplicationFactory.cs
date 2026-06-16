using DocumentVerifier.Application.Interfaces;
using DocumentVerifier.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentVerifier.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "InMemory",
                ["Database:InMemoryName"] = $"document-verifier-{Guid.NewGuid()}",
                ["Blockchain:UseMock"] = "true",
                ["Blockchain:ContractAddress"] = "0x0000000000000000000000000000000000000000",
                ["Blockchain:NetworkName"] = "mock-local"
            });
        });

        builder.ConfigureServices(services =>
        {
            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            DatabaseSeeder.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        });
    }
}
