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
    private readonly ILogger<ServiceRequestsController> _logger;

    public ServiceRequestsController(
        ITechmoveApiClient apiClient,
        IExchangeRateService exchangeRateService,
        ILogger<ServiceRequestsController> logger)
    {
        _apiClient = apiClient;
        _exchangeRateService = exchangeRateService;
        _logger = logger;
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

        IReadOnlyList<ServiceRequestViewModel> requests;
        try
        {
            requests = await _apiClient.GetServiceRequestsAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to load service requests from the Techmove API");
            TempData["ServiceRequestError"] = ex.Message;
            requests = [];
        }

        var model = new ServiceRequestsIndexViewModel
        {
            Requests = requests,
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

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var request = await _apiClient.GetServiceRequestAsync(id, cancellationToken);
        return request is null ? NotFound() : View(request);
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
        request.ContractId = contract.Id;
        try
        {
            await _apiClient.SaveServiceRequestAsync(request, contractId, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to create service request for contract {ContractId}", contractId);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while creating a service request");
            ModelState.AddModelError(string.Empty, "The service request could not be saved because the API is unavailable. Please try again.");
            return View(request);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var request = await _apiClient.GetServiceRequestAsync(id, cancellationToken);
        if (request is null)
        {
            return NotFound();
        }

        ViewBag.Contracts = await _apiClient.GetContractsAsync(cancellationToken);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceRequestViewModel request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }

        var contracts = await _apiClient.GetContractsAsync(cancellationToken);
        ViewBag.Contracts = contracts;
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var contract = contracts.FirstOrDefault(c => c.Id == request.ContractId);
        if (contract is null)
        {
            ModelState.AddModelError(string.Empty, "Please select a valid contract.");
            return View(request);
        }

        if (contract.Status is "Expired" or "On Hold")
        {
            ModelState.AddModelError(string.Empty, "A service request cannot be linked to Expired or On Hold contracts.");
            return View(request);
        }

        request.ContractRef = $"CT-{contract.Id} - {contract.ClientName} ({contract.Status})";
        try
        {
            await _apiClient.UpdateServiceRequestAsync(id, request, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to update service request {ServiceRequestId}", id);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while updating service request {ServiceRequestId}", id);
            ModelState.AddModelError(string.Empty, "The service request could not be updated because the API is unavailable. Please try again.");
            return View(request);
        }
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var request = await _apiClient.GetServiceRequestAsync(id, cancellationToken);
        return request is null ? NotFound() : View(request);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteServiceRequestAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to delete service request {ServiceRequestId}", id);
            TempData["ServiceRequestError"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while deleting service request {ServiceRequestId}", id);
            TempData["ServiceRequestError"] = "The service request could not be deleted because the API is unavailable. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }
}
