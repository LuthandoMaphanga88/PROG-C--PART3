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
}
