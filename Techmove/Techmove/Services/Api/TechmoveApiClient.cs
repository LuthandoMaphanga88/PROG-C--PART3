using System.Net.Http.Headers;
using System.Net.Http.Json;
using Techmove.Models;

namespace Techmove.Services.Api;

public class TechmoveApiClient : ITechmoveApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public TechmoveApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<ClientViewModel>> GetClientsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<ClientViewModel>>("api/clients", cancellationToken) ?? [];
    }

    public async Task<ClientViewModel?> GetClientByAccountUsernameAsync(string accountUsername, CancellationToken cancellationToken = default)
    {
        return await GetAsync<ClientViewModel>($"api/clients/by-account/{Uri.EscapeDataString(accountUsername)}", cancellationToken);
    }

    public async Task SaveClientAsync(ClientViewModel client, CancellationToken cancellationToken = default)
    {
        await PostAsync("api/clients", client, cancellationToken);
    }

    public async Task SaveClientProfileAsync(string accountUsername, ClientViewModel client, CancellationToken cancellationToken = default)
    {
        await PutAsync($"api/clients/by-account/{Uri.EscapeDataString(accountUsername)}", client, cancellationToken);
    }

    public async Task<IReadOnlyList<ContractViewModel>> GetContractsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<ContractViewModel>>("api/contracts", cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<ContractViewModel>> SearchContractsAsync(
        string? name,
        string? client,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        AddQuery(query, "name", name);
        AddQuery(query, "client", client);
        AddQuery(query, "startDate", startDate?.ToString("O"));
        AddQuery(query, "endDate", endDate?.ToString("O"));

        var path = query.Count == 0 ? "api/contracts" : $"api/contracts?{string.Join("&", query)}";
        return await GetAsync<List<ContractViewModel>>(path, cancellationToken) ?? [];
    }

    public async Task<ContractViewModel?> GetContractAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetAsync<ContractViewModel>($"api/contracts/{id}", cancellationToken);
    }

    public async Task SaveContractAsync(ContractViewModel contract, CancellationToken cancellationToken = default)
    {
        await PostAsync("api/contracts", contract, cancellationToken);
    }

    public async Task SaveReturnedAgreementAsync(int contractId, string storedFileName, CancellationToken cancellationToken = default)
    {
        await PatchAsync($"api/contracts/{contractId}/returned-agreement", new { storedFileName }, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceRequestViewModel>> GetServiceRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<ServiceRequestViewModel>>("api/service-requests", cancellationToken) ?? [];
    }

    public async Task SaveServiceRequestAsync(ServiceRequestViewModel request, int contractId, CancellationToken cancellationToken = default)
    {
        await PostAsync("api/service-requests", new ServiceRequestDto
        {
            ContractId = contractId,
            ContractRef = request.ContractRef,
            Description = request.Description,
            CostUsd = request.CostUsd,
            CostZar = request.CostZar,
            Status = request.Status
        }, cancellationToken);
    }

    private async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, requestUri);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private async Task PostAsync<T>(string requestUri, T body, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Post, requestUri);
        request.Content = JsonContent.Create(body);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task PutAsync<T>(string requestUri, T body, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Put, requestUri);
        request.Content = JsonContent.Create(body);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task PatchAsync<T>(string requestUri, T body, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Patch, requestUri);
        request.Content = JsonContent.Create(body);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
    {
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JwtTokenFactory.CreateToken(_configuration));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private static void AddQuery(List<string> query, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
        }
    }

    private sealed class ServiceRequestDto
    {
        public int ContractId { get; set; }
        public string ContractRef { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostUsd { get; set; }
        public decimal CostZar { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
