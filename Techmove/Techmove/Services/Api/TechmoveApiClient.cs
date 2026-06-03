using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Techmove.Models;

namespace Techmove.Services.Api;

public class TechmoveApiClient : ITechmoveApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TechmoveApiClient> _logger;

    public TechmoveApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TechmoveApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ClientViewModel>> GetClientsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<ClientViewModel>>("api/clients", cancellationToken) ?? [];
    }

    public async Task<ClientViewModel?> GetClientAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetAsync<ClientViewModel>($"api/clients/{id}", cancellationToken);
    }

    public async Task<ClientViewModel?> GetClientByAccountUsernameAsync(string accountUsername, CancellationToken cancellationToken = default)
    {
        return await GetAsync<ClientViewModel>($"api/clients/by-account/{Uri.EscapeDataString(accountUsername)}", cancellationToken);
    }

    public async Task SaveClientAsync(ClientViewModel client, CancellationToken cancellationToken = default)
    {
        await PostAsync("api/clients", client, cancellationToken);
    }

    public async Task UpdateClientAsync(int id, ClientViewModel client, CancellationToken cancellationToken = default)
    {
        await PutAsync($"api/clients/{id}", client, cancellationToken);
    }

    public async Task DeleteClientAsync(int id, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"api/clients/{id}", cancellationToken);
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

    public async Task UpdateContractAsync(int id, ContractViewModel contract, CancellationToken cancellationToken = default)
    {
        await PutAsync($"api/contracts/{id}", contract, cancellationToken);
    }

    public async Task DeleteContractAsync(int id, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"api/contracts/{id}", cancellationToken);
    }

    public async Task SaveReturnedAgreementAsync(int contractId, string storedFileName, CancellationToken cancellationToken = default)
    {
        await PatchAsync($"api/contracts/{contractId}/returned-agreement", new { storedFileName }, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceRequestViewModel>> GetServiceRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<ServiceRequestViewModel>>("api/service-requests", cancellationToken) ?? [];
    }

    public async Task<ServiceRequestViewModel?> GetServiceRequestAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetAsync<ServiceRequestViewModel>($"api/service-requests/{id}", cancellationToken);
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

    public async Task UpdateServiceRequestAsync(int id, ServiceRequestViewModel request, CancellationToken cancellationToken = default)
    {
        await PutAsync($"api/service-requests/{id}", request, cancellationToken);
    }

    public async Task DeleteServiceRequestAsync(int id, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"api/service-requests/{id}", cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            var apiAddress = _httpClient.BaseAddress?.ToString() ?? "the configured Techmove API";
            var message = cancellationToken.IsCancellationRequested
                ? $"The request to {apiAddress} was canceled before it completed."
                : $"The Techmove API at {apiAddress} did not respond in time. Start Techmove.API (https://localhost:7000) and try again.";

            _logger.LogError(ex, "Techmove API request to {RequestUri} timed out or was canceled", request.RequestUri);
            throw new HttpRequestException(message, ex);
        }
        catch (HttpRequestException ex)
        {
            var apiAddress = _httpClient.BaseAddress?.ToString() ?? "the configured Techmove API";
            _logger.LogError(ex, "Unable to reach Techmove API at {ApiAddress}", apiAddress);
            throw new HttpRequestException(
                $"Unable to reach the Techmove API at {apiAddress}. Start Techmove.API and try again.",
                ex);
        }
    }

    private async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, requestUri);
        using var response = await SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessfulResponseAsync(response, requestUri, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private async Task PostAsync<T>(string requestUri, T body, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Post, requestUri);
        request.Content = JsonContent.Create(body);
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessfulResponseAsync(response, requestUri, cancellationToken);
    }

    private async Task PutAsync<T>(string requestUri, T body, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Put, requestUri);
        request.Content = JsonContent.Create(body);
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessfulResponseAsync(response, requestUri, cancellationToken);
    }

    private async Task PatchAsync<T>(string requestUri, T body, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Patch, requestUri);
        request.Content = JsonContent.Create(body);
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessfulResponseAsync(response, requestUri, cancellationToken);
    }

    private async Task DeleteAsync(string requestUri, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Delete, requestUri);
        using var response = await SendAsync(request, cancellationToken);
        await EnsureSuccessfulResponseAsync(response, requestUri, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
    {
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JwtTokenFactory.CreateToken(_configuration));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private async Task EnsureSuccessfulResponseAsync(
        HttpResponseMessage response,
        string requestUri,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = TryReadErrorMessage(responseBody)
            ?? $"The Techmove API returned {(int)response.StatusCode} ({response.ReasonPhrase}).";

        _logger.LogWarning(
            "Techmove API request to {RequestUri} failed with status {StatusCode}: {Message}",
            requestUri,
            (int)response.StatusCode,
            message);

        throw new TechmoveApiException(message, (int)response.StatusCode, responseBody);
    }

    private static string? TryReadErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
            {
                return message.GetString();
            }

            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
            {
                return detail.GetString();
            }

            if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
            {
                return title.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
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
