namespace Techmove.Services;

public interface IExchangeRateService
{
    Task<IReadOnlyDictionary<string, decimal>> GetExchangeRatesAsync(
        string baseCurrency,
        IEnumerable<string> targetCurrencies,
        CancellationToken cancellationToken = default);
}
