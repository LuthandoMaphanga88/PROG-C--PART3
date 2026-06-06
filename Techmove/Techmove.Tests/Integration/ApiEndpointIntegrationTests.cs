using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Techmove.Tests.Integration;

public class ApiEndpointIntegrationTests : IClassFixture<TechmoveApiFactory>
{
    private readonly TechmoveApiFactory _factory;

    public ApiEndpointIntegrationTests(TechmoveApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/api/contracts")]
    [InlineData("/api/clients")]
    [InlineData("/api/service-requests")]
    public async Task GetEndpoint_WithValidBearerToken_ReturnsOkAndJsonBody(string endpoint)
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync(endpoint);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.True(json.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetContracts_WithoutBearerToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/contracts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostClient_WithValidData_ReturnsCreatedAndClientIsRetrievable()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var newClient = new
        {
            AccountUsername = "integration-new-client",
            Name = "Integration New Client",
            ContactDetails = "new-client@example.com",
            Region = "KwaZulu-Natal"
        };

        var response = await client.PostAsJsonAsync("/api/clients", newClient);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("integration-new-client", created.GetProperty("accountUsername").GetString());
        Assert.Equal("Integration New Client", created.GetProperty("name").GetString());

        var location = response.Headers.Location;
        Assert.NotNull(location);

        var getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("integration-new-client", retrieved.GetProperty("accountUsername").GetString());
    }

    [Fact]
    public async Task PostServiceRequest_WithValidContract_ReturnsCreatedAndCanBeLoaded()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var newRequest = new
        {
            ContractId = 1,
            ContractRef = "CT-1 - Integration Client (Active)",
            Description = "Integration created request",
            CostUsd = 150.00m,
            CostZar = 2775.00m,
            Status = "Open"
        };

        var response = await client.PostAsJsonAsync("/api/service-requests", newRequest);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Integration created request", created.GetProperty("description").GetString());
        Assert.Equal(150.00m, created.GetProperty("costUsd").GetDecimal());

        var location = response.Headers.Location;
        Assert.NotNull(location);

        var getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Integration created request", retrieved.GetProperty("description").GetString());
    }
}
