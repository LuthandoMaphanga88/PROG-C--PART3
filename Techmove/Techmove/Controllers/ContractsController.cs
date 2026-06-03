using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services.Api;

namespace Techmove.Controllers;

[Authorize]
public class ContractsController : Controller
{
    private static readonly HashSet<string> AllowedReturnedAgreementExtensions =
    [
        ".pdf",
        ".doc",
        ".docx"
    ];

    private readonly ITechmoveApiClient _apiClient;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ContractsController(ITechmoveApiClient apiClient, IWebHostEnvironment hostEnvironment)
    {
        _apiClient = apiClient;
        _hostEnvironment = hostEnvironment;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var contracts = await _apiClient.GetContractsAsync(cancellationToken);
        return View(contracts);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Clients = await _apiClient.GetClientsAsync(cancellationToken);
        return View(new ContractViewModel
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(12),
            Status = "Draft",
            ServiceLevel = "Gold SLA"
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractViewModel contract, IFormFile? signedAgreement, CancellationToken cancellationToken)
    {
        var clients = await _apiClient.GetClientsAsync(cancellationToken);
        ViewBag.Clients = clients;
        var selectedClient = clients.FirstOrDefault(c => c.Name == contract.ClientName);

        if (selectedClient is null)
        {
            ModelState.AddModelError(nameof(contract.ClientName), "Please select a valid client.");
        }

        if (contract.StartDate == default || contract.EndDate == default || contract.EndDate < contract.StartDate)
        {
            ModelState.AddModelError(nameof(contract.EndDate), "End date must be the same as or after start date.");
        }

        if (!ModelState.IsValid)
        {
            return View(contract);
        }

        if (signedAgreement is not null && signedAgreement.Length > 0)
        {
            var uploadResult = await SaveUploadedFileAsync(
                signedAgreement,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf" });

            if (!uploadResult.IsValid)
            {
                ModelState.AddModelError(string.Empty, uploadResult.ErrorMessage!);
                return View(contract);
            }

            contract.AgreementFileName = uploadResult.StoredFileName!;
        }

        contract.ClientAccountUsername = selectedClient!.AccountUsername;
        await _apiClient.SaveContractAsync(contract, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Search(string? name, string? client, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var allContracts = await _apiClient.GetContractsAsync(cancellationToken);
        var results = await _apiClient.SearchContractsAsync(name, client, startDate, endDate, cancellationToken);

        var model = new ContractSearchViewModel
        {
            Name = name ?? string.Empty,
            Client = client ?? string.Empty,
            StartDate = startDate,
            EndDate = endDate,
            ClientOptions = allContracts
                .Select(c => c.ClientName)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList(),
            Results = results.ToList()
        };

        return View(model);
    }

    [Authorize(Roles = "Client")]
    public async Task<IActionResult> MyContracts(CancellationToken cancellationToken)
    {
        var accountUsername = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(accountUsername))
        {
            return Challenge();
        }

        var contracts = (await _apiClient.GetContractsAsync(cancellationToken))
            .Where(contract => string.Equals(contract.ClientAccountUsername, accountUsername, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return View(contracts);
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadReturnedAgreement(int contractId, IFormFile? returnedAgreement, CancellationToken cancellationToken)
    {
        var accountUsername = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(accountUsername))
        {
            return Challenge();
        }

        var contract = await _apiClient.GetContractAsync(contractId, cancellationToken);
        if (contract is null ||
            !string.Equals(contract.ClientAccountUsername, accountUsername, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ContractUploadError"] = "Contract not found.";
            return RedirectToAction(nameof(MyContracts));
        }

        if (returnedAgreement is null || returnedAgreement.Length == 0)
        {
            TempData["ContractUploadError"] = "Please choose a file to upload.";
            return RedirectToAction(nameof(MyContracts));
        }

        var uploadResult = await SaveUploadedFileAsync(returnedAgreement, AllowedReturnedAgreementExtensions);
        if (!uploadResult.IsValid)
        {
            TempData["ContractUploadError"] = uploadResult.ErrorMessage!;
            return RedirectToAction(nameof(MyContracts));
        }

        await _apiClient.SaveReturnedAgreementAsync(contractId, uploadResult.StoredFileName!, cancellationToken);
        TempData["ContractUploadSuccess"] = "Returned agreement uploaded successfully.";
        return RedirectToAction(nameof(MyContracts));
    }

    private async Task<FileSaveResult> SaveUploadedFileAsync(
        IFormFile file,
        HashSet<string> allowedExtensions)
    {
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
        {
            var allowed = string.Join(", ", allowedExtensions.OrderBy(x => x));
            return FileSaveResult.Invalid($"Unsupported file type. Allowed: {allowed}.");
        }

        var uploadsDirectory = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDirectory);

        var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(uploadsDirectory, safeFileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        return FileSaveResult.Valid(safeFileName);
    }

    private sealed record FileSaveResult(bool IsValid, string? StoredFileName, string? ErrorMessage)
    {
        public static FileSaveResult Valid(string fileName) => new(true, fileName, null);
        public static FileSaveResult Invalid(string message) => new(false, null, message);
    }
}
