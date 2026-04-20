using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services;

namespace Techmove.Controllers;

[Authorize]
public class ContractsController : Controller
{
    private readonly InMemoryDataStore _dataStore;
    private readonly IWebHostEnvironment _environment;

    public ContractsController(InMemoryDataStore dataStore, IWebHostEnvironment environment)
    {
        _dataStore = dataStore;
        _environment = environment;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Index()
    {
        return View(_dataStore.Contracts);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Search(string? name, string? client, DateTime? startDate, DateTime? endDate)
    {
        var contracts = _dataStore.Contracts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            contracts = contracts.Where(contract =>
                contract.ServiceLevel.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                contract.ClientName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                $"CT-{contract.Id}".Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(client))
        {
            contracts = contracts.Where(contract =>
                string.Equals(contract.ClientName, client, StringComparison.OrdinalIgnoreCase));
        }

        if (startDate.HasValue)
        {
            contracts = contracts.Where(contract => contract.StartDate.Date >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            contracts = contracts.Where(contract => contract.EndDate.Date <= endDate.Value.Date);
        }

        var model = new ContractSearchViewModel
        {
            Name = name ?? string.Empty,
            Client = client ?? string.Empty,
            StartDate = startDate,
            EndDate = endDate,
            ClientOptions = _dataStore.Contracts
                .Select(contract => contract.ClientName)
                .Where(clientName => !string.IsNullOrWhiteSpace(clientName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(clientName => clientName)
                .ToList(),
            Results = contracts
                .OrderByDescending(contract => contract.StartDate)
                .ToList()
        };

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewBag.Clients = _dataStore.Clients;
        return View(new ContractViewModel());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractViewModel contract, IFormFile? signedAgreement)
    {
        ViewBag.Clients = _dataStore.Clients;
        if (!ModelState.IsValid)
        {
            return View(contract);
        }

        var linkedClient = _dataStore.Clients.FirstOrDefault(client =>
            string.Equals(client.Name, contract.ClientName, StringComparison.OrdinalIgnoreCase));
        if (linkedClient is not null)
        {
            contract.ClientAccountUsername = linkedClient.AccountUsername;
        }

        if (signedAgreement is not null && signedAgreement.Length > 0)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);
            var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(signedAgreement.FileName)}";
            var fullPath = Path.Combine(uploadsPath, fileName);
            await using var stream = System.IO.File.Create(fullPath);
            await signedAgreement.CopyToAsync(stream);
            contract.AgreementFileName = fileName;
        }

        _dataStore.AddContract(contract);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Client")]
    public IActionResult MyContracts()
    {
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var contracts = _dataStore.GetContractsByClientAccountUsername(username);
        return View(contracts);
    }

    [Authorize(Roles = "Client")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadReturnedAgreement(int contractId, IFormFile? returnedAgreement)
    {
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var contract = _dataStore.GetContractById(contractId);
        if (contract is null || !string.Equals(contract.ClientAccountUsername, username, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        if (returnedAgreement is null || returnedAgreement.Length == 0)
        {
            TempData["ContractUploadError"] = "Please choose a file before submitting.";
            return RedirectToAction(nameof(MyContracts));
        }

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);
        var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(returnedAgreement.FileName)}";
        var fullPath = Path.Combine(uploadsPath, fileName);
        await using var stream = System.IO.File.Create(fullPath);
        await returnedAgreement.CopyToAsync(stream);

        contract.ClientReturnedAgreementFileName = fileName;
        TempData["ContractUploadSuccess"] = "Your contract has been submitted.";
        return RedirectToAction(nameof(MyContracts));
    }
}
