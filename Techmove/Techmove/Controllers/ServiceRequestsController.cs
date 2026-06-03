using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services;
using Techmove.Services.Api;

namespace Techmove.Controllers;

[Authorize]
public class ServiceRequestsController : Controller
{
    private readonly ITechmoveApiClient _apiClient;
    private readonly IExchangeRateService _exchangeRateService;

    public ServiceRequestsController(ITechmoveApiClient apiClient, IExchangeRateService exchangeRateService)
    {
        _apiClient = apiClient;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        decimal eurRate = 0m;
        decimal gbpRate = 0m;

        try
        {
            var rates = await _exchangeRateService.GetExchangeRatesAsync(
                "USD",
                ["EUR", "GBP"],
                cancellationToken);
            eurRate = rates.GetValueOrDefault("EUR");
            gbpRate = rates.GetValueOrDefault("GBP");
        }
        catch
        {
            // Gracefully fall back to zeroed comparison values if the API is unavailable.
        }

        var model = new ServiceRequestsIndexViewModel
        {
            Requests = await _apiClient.GetServiceRequestsAsync(cancellationToken),
            EurRate = eurRate,
            GbpRate = gbpRate
        };

        return View(model);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Contracts = await _apiClient.GetContractsAsync(cancellationToken);
        return View(new ServiceRequestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestViewModel request, int contractId, CancellationToken cancellationToken)
    {
        var contracts = await _apiClient.GetContractsAsync(cancellationToken);
        ViewBag.Contracts = contracts;
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var contract = contracts.FirstOrDefault(c => c.Id == contractId);
        if (contract is null)
        {
            ModelState.AddModelError(string.Empty, "Please select a valid contract.");
            return View(request);
        }

        if (contract.Status is "Expired" or "On Hold")
        {
            ModelState.AddModelError(string.Empty, "A service request cannot be created for Expired or On Hold contracts.");
            return View(request);
        }

        request.ContractRef = $"CT-{contract.Id} - {contract.ClientName} ({contract.Status})";
        await _apiClient.SaveServiceRequestAsync(request, contractId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
