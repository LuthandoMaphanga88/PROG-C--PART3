// Example: How to Use the API from the MVC Frontend
// This file shows how to refactor MVC Controllers to use the API instead of direct database access

using System.Text;
using System.Text.Json;

namespace Techmove.Services;

/// <summary>
/// Service to communicate with the Techmove API from the MVC frontend.
/// Inject this into your MVC controllers to replace direct database access.
/// </summary>
public interface IContractApiService
{
    Task<List<ContractDto>> GetContractsAsync(string? status = null, int? clientId = null);
    Task<ContractDto?> GetContractAsync(int id);
    Task<ContractDto> CreateContractAsync(ContractDto contract);
    Task<ContractDto> UpdateContractStatusAsync(int id, string status);
}

public class ContractApiService : IContractApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContractApiService> _logger;
    private readonly IConfiguration _configuration;

    public ContractApiService(HttpClient httpClient, ILogger<ContractApiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;

        // Set base address from configuration
        var apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:5001";
        _httpClient.BaseAddress = new Uri(apiBaseUrl);
    }

    public async Task<List<ContractDto>> GetContractsAsync(string? status = null, int? clientId = null)
    {
        try
        {
            var query = "/api/contracts";
            var parameters = new List<string>();

            if (!string.IsNullOrEmpty(status))
                parameters.Add($"status={Uri.EscapeDataString(status)}");

            if (clientId.HasValue)
                parameters.Add($"clientId={clientId}");

            if (parameters.Count > 0)
                query += "?" + string.Join("&", parameters);

            var response = await _httpClient.GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var contracts = JsonSerializer.Deserialize<List<ContractDto>>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return contracts ?? [];
            }

            _logger.LogError("API returned status {StatusCode} when getting contracts", response.StatusCode);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contracts from API");
            throw;
        }
    }

    public async Task<ContractDto?> GetContractAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/contracts/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ContractDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            _logger.LogError("API returned status {StatusCode} when getting contract {Id}", response.StatusCode, id);
            throw new HttpRequestException($"API returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract {Id} from API", id);
            throw;
        }
    }

    public async Task<ContractDto> CreateContractAsync(ContractDto contract)
    {
        try
        {
            var json = JsonSerializer.Serialize(contract);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/contracts", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var createdContract = JsonSerializer.Deserialize<ContractDto>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return createdContract!;
            }

            _logger.LogError("API returned status {StatusCode} when creating contract", response.StatusCode);
            throw new HttpRequestException($"API returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contract via API");
            throw;
        }
    }

    public async Task<ContractDto> UpdateContractStatusAsync(int id, string status)
    {
        try
        {
            var updateDto = new { status };
            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/contracts/{id}/status")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var updatedContract = JsonSerializer.Deserialize<ContractDto>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return updatedContract!;
            }

            _logger.LogError("API returned status {StatusCode} when updating contract {Id} status", response.StatusCode, id);
            throw new HttpRequestException($"API returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract {Id} status via API", id);
            throw;
        }
    }
}

// DTOs for API communication
namespace Techmove.API.Models;

public class ContractDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientAccountUsername { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceLevel { get; set; } = string.Empty;
    public string AgreementFileName { get; set; } = string.Empty;
    public string ClientReturnedAgreementFileName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/*
 * USAGE IN MVC CONTROLLER:
 * 
 * 1. Register the service in Program.cs:
 *    builder.Services.AddHttpClient<IContractApiService, ContractApiService>();
 * 
 * 2. Add ApiBaseUrl to appsettings.json:
 *    {
 *      "ApiBaseUrl": "https://localhost:5001"
 *    }
 * 
 * 3. Inject into your MVC controller:
 *    public class ContractsController : Controller
 *    {
 *        private readonly IContractApiService _apiService;
 *        
 *        public ContractsController(IContractApiService apiService)
 *        {
 *            _apiService = apiService;
 *        }
 *        
 *        public async Task<IActionResult> Index(string? status = null)
 *        {
 *            var contracts = await _apiService.GetContractsAsync(status);
 *            return View(contracts);
 *        }
 *    }
 */
