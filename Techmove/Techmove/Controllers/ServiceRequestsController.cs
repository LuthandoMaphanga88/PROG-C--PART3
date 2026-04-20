using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services;

namespace Techmove.Controllers;

[Authorize]
public class ServiceRequestsController : Controller
{
    private readonly InMemoryDataStore _dataStore;
    private readonly IExchangeRateService _exchangeRateService;

    public ServiceRequestsController(InMemoryDataStore dataStore, IExchangeRateService exchangeRateService)
    {
        _dataStore = dataStore;
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
            Requests = _dataStore.ServiceRequests,
            EurRate = eurRate,
            GbpRate = gbpRate
        };

        return View(model);
    }

    public IActionResult Create()
    {
        ViewBag.Contracts = _dataStore.Contracts;
        return View(new ServiceRequestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ServiceRequestViewModel request, int contractId)
    {
        ViewBag.Contracts = _dataStore.Contracts;
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var contract = _dataStore.Contracts.FirstOrDefault(c => c.Id == contractId);
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
        _dataStore.AddServiceRequest(request);
        return RedirectToAction(nameof(Index));
    }
}
