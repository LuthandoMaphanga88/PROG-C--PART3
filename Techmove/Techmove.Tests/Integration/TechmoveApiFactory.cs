using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Techmove.API;
using Techmove.Data;
using Techmove.Models;

namespace Techmove.Tests.Integration;

public class TechmoveApiFactory : WebApplicationFactory<ApiAssemblyMarker>
{
    private const string Issuer = "Techmove.Mvc";
    private const string Audience = "Techmove.Api";
    private const string SigningKey = "Integration-Test-Signing-Key-For-Techmove-Api";
    private readonly string _databaseName = $"TechmoveApiTests-{Guid.NewGuid()}";
    private readonly InMemoryDatabaseRoot _databaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ApiAuthentication:Issuer", Issuer);
        builder.UseSetting("ApiAuthentication:Audience", Audience);
        builder.UseSetting("ApiAuthentication:SigningKey", SigningKey);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName, _databaseRoot));

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            SeedDatabase(context);
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());
        return client;
    }

    private static void SeedDatabase(AppDbContext context)
    {
        var now = DateTime.UtcNow;
        var client = new Client
        {
            Id = 1,
            AccountUsername = "integration-client",
            Name = "Integration Client",
            ContactDetails = "integration@example.com",
            Region = "Gauteng",
            CreatedDate = now,
            ModifiedDate = now
        };

        var contract = new Contract
        {
            Id = 1,
            ClientId = client.Id,
            ClientName = client.Name,
            ClientAccountUsername = client.AccountUsername,
            StartDate = now.Date,
            EndDate = now.Date.AddMonths(12),
            Status = "Active",
            ServiceLevel = "Premium",
            AgreementFileName = "integration-agreement.pdf",
            CreatedDate = now,
            ModifiedDate = now
        };

        var serviceRequest = new ServiceRequest
        {
            Id = 1,
            ContractId = contract.Id,
            ContractRef = "CT-1 - Integration Client (Active)",
            Description = "Integration test request",
            CostUsd = 100,
            CostZar = 1850,
            Status = "Open",
            CreatedDate = now,
            ModifiedDate = now
        };

        context.Clients.Add(client);
        context.Contracts.Add(contract);
        context.ServiceRequests.Add(serviceRequest);
        context.SaveChanges();
    }

    private static string CreateToken()
    {
        var now = DateTimeOffset.UtcNow;
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object>
        {
            ["iss"] = Issuer,
            ["aud"] = Audience,
            ["sub"] = "techmove-integration-tests",
            ["iat"] = now.ToUnixTimeSeconds(),
            ["nbf"] = now.ToUnixTimeSeconds(),
            ["exp"] = now.AddMinutes(30).ToUnixTimeSeconds()
        };

        var unsignedToken = $"{Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header))}.{Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload))}";
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(SigningKey), Encoding.UTF8.GetBytes(unsignedToken));
        return $"{unsignedToken}.{Base64UrlEncode(signature)}";
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
