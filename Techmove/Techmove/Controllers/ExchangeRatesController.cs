using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Services;

namespace Techmove.Controllers;

[ApiController]
[Route("api/exchange-rates")]
[Authorize]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public ExchangeRatesController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string baseCurrency = "USD",
        [FromQuery] string targets = "EUR,GBP,CAD",
        CancellationToken cancellationToken = default)
    {
        var targetCurrencies = targets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var rates = await _exchangeRateService.GetExchangeRatesAsync(baseCurrency, targetCurrencies, cancellationToken);

        return Ok(new
        {
            BaseCurrency = baseCurrency.ToUpperInvariant(),
            Rates = rates
        });
    }
}
