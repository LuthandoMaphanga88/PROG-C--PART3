using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Techmove.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExchangeRateService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IReadOnlyDictionary<string, decimal>> GetExchangeRatesAsync(
        string baseCurrency,
        IEnumerable<string> targetCurrencies,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["ExchangeRateApi:ApiKey"];
        var baseUrl = _configuration["ExchangeRateApi:BaseUrl"] ?? "https://v6.exchangerate-api.com";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("ExchangeRateApi:ApiKey is missing in configuration.");
        }

        var normalizedBaseCurrency = baseCurrency.Trim().ToUpperInvariant();
        var url = $"{baseUrl.TrimEnd('/')}/v6/{apiKey}/latest/{normalizedBaseCurrency}";
        ExchangeRateApiResponse? response;
        try
        {
            response = await _httpClient.GetFromJsonAsync<ExchangeRateApiResponse>(url, cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            throw new HttpRequestException("Exchange rate request timed out or was canceled.", ex);
        }

        if (response is null || !string.Equals(response.Result, "success", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Unable to retrieve exchange rates from ExchangeRate-API.");
        }

        var requestedTargets = targetCurrencies
            .Select(currency => currency.Trim().ToUpperInvariant())
            .Where(currency => !string.IsNullOrWhiteSpace(currency))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (requestedTargets.Count == 0)
        {
            return new Dictionary<string, decimal>();
        }

        var results = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var target in requestedTargets)
        {
            if (response.ConversionRates.TryGetValue(target, out var rate))
            {
                results[target] = rate;
            }
        }

        return results;
    }

    private sealed class ExchangeRateApiResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = string.Empty;

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
