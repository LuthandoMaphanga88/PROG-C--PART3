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
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        ITechmoveApiClient apiClient,
        IWebHostEnvironment hostEnvironment,
        ILogger<ContractsController> logger)
    {
        _apiClient = apiClient;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var contracts = await _apiClient.GetContractsAsync(cancellationToken);
            return View(contracts);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to load contracts from the Techmove API");
            TempData["ContractError"] = ex.Message;
            return View(Array.Empty<ContractViewModel>());
        }
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

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var contract = await _apiClient.GetContractAsync(id, cancellationToken);
        return contract is null ? NotFound() : View(contract);
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
        try
        {
            await _apiClient.SaveContractAsync(contract, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to create contract for {ClientName}", contract.ClientName);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(contract);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while creating a contract");
            ModelState.AddModelError(string.Empty, "The contract could not be saved because the API is unavailable. Please try again.");
            return View(contract);
        }
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

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var contract = await _apiClient.GetContractAsync(id, cancellationToken);
        if (contract is null)
        {
            return NotFound();
        }

        ViewBag.Clients = await _apiClient.GetClientsAsync(cancellationToken);
        return View(contract);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContractViewModel contract, IFormFile? signedAgreement, CancellationToken cancellationToken)
    {
        if (id != contract.Id)
        {
            return BadRequest();
        }

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
        try
        {
            await _apiClient.UpdateContractAsync(id, contract, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to update contract {ContractId}", id);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(contract);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while updating contract {ContractId}", id);
            ModelState.AddModelError(string.Empty, "The contract could not be updated because the API is unavailable. Please try again.");
            return View(contract);
        }
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var contract = await _apiClient.GetContractAsync(id, cancellationToken);
        return contract is null ? NotFound() : View(contract);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteContractAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to delete contract {ContractId}", id);
            TempData["ContractError"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while deleting contract {ContractId}", id);
            TempData["ContractError"] = "The contract could not be deleted because the API is unavailable. Please try again.";
            return RedirectToAction(nameof(Index));
        }
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

        try
        {
            await _apiClient.SaveReturnedAgreementAsync(contractId, uploadResult.StoredFileName!, cancellationToken);
            TempData["ContractUploadSuccess"] = "Returned agreement uploaded successfully.";
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to save returned agreement metadata for contract {ContractId}", contractId);
            TempData["ContractUploadError"] = ex.Message;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while uploading returned agreement for contract {ContractId}", contractId);
            TempData["ContractUploadError"] = "The returned agreement was uploaded, but the contract could not be updated because the API is unavailable.";
        }

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

        try
        {
            var uploadsDirectory = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsDirectory);

            var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var fullPath = Path.Combine(uploadsDirectory, safeFileName);

            await using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);

            return FileSaveResult.Valid(safeFileName);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Unable to save uploaded file {FileName}", file.FileName);
            return FileSaveResult.Invalid("The file could not be saved. Please try again.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Upload directory is not writable for file {FileName}", file.FileName);
            return FileSaveResult.Invalid("The upload folder is not available. Please contact support.");
        }
    }

    private sealed record FileSaveResult(bool IsValid, string? StoredFileName, string? ErrorMessage)
    {
        public static FileSaveResult Valid(string fileName) => new(true, fileName, null);
        public static FileSaveResult Invalid(string message) => new(false, null, message);
    }
}
