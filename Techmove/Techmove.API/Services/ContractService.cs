using Microsoft.EntityFrameworkCore;
using Techmove.Data;
using Techmove.Models;
using Techmove.API.Models;

namespace Techmove.API.Services;

/// <summary>
/// Implementation of contract service for managing contract business logic.
/// This service handles all contract-related database operations.
/// </summary>
public class ContractService : IContractService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ContractService> _logger;

    public ContractService(AppDbContext context, ILogger<ContractService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ContractDto>> GetContractsAsync(
        string? status = null,
        int? clientId = null,
        string? name = null,
        string? client = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            if (clientId.HasValue)
            {
                query = query.Where(c => c.ClientId == clientId.Value);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var normalizedName = name.Trim();
                query = query.Where(c =>
                    c.ClientName.Contains(normalizedName) ||
                    c.ServiceLevel.Contains(normalizedName) ||
                    ("CT-" + c.Id).Contains(normalizedName));
            }

            if (!string.IsNullOrWhiteSpace(client))
            {
                var normalizedClient = client.Trim();
                query = query.Where(c => c.ClientName == normalizedClient);
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate.Date <= endDate.Value.Date);
            }

            var contracts = await query
                .OrderByDescending(c => c.StartDate)
                .ThenBy(c => c.ClientName)
                .ToListAsync();

            return contracts.Select(MapToContractDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contracts with status: {Status}, clientId: {ClientId}", status, clientId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ContractDto?> GetContractByIdAsync(int id)
    {
        try
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            return contract != null ? MapToContractDto(contract) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract with ID: {ContractId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ContractDto> CreateContractAsync(ContractDto contractDto)
    {
        try
        {
            var client = await ResolveClientAsync(contractDto);
            if (client is null)
            {
                _logger.LogWarning("Attempt to create contract for an unknown client");
                throw new InvalidOperationException("Client not found. Provide a valid clientId, clientName, or clientAccountUsername.");
            }

            var contract = new Contract
            {
                ClientId = client.Id,
                ClientName = client.Name,
                ClientAccountUsername = client.AccountUsername,
                StartDate = contractDto.StartDate,
                EndDate = contractDto.EndDate,
                Status = string.IsNullOrWhiteSpace(contractDto.Status) ? "Draft" : contractDto.Status.Trim(),
                ServiceLevel = contractDto.ServiceLevel,
                AgreementFileName = contractDto.AgreementFileName,
                ClientReturnedAgreementFileName = contractDto.ClientReturnedAgreementFileName,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract created successfully with ID: {ContractId}", contract.Id);
            return MapToContractDto(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contract");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ContractDto?> UpdateContractAsync(int id, ContractDto contractDto)
    {
        try
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract is null)
            {
                _logger.LogWarning("Attempt to update non-existent contract with ID: {ContractId}", id);
                return null;
            }

            var client = await ResolveClientAsync(contractDto);
            if (client is null)
            {
                _logger.LogWarning("Attempt to update contract {ContractId} with an unknown client", id);
                throw new InvalidOperationException("Client not found. Provide a valid clientId, clientName, or clientAccountUsername.");
            }

            contract.ClientId = client.Id;
            contract.ClientName = client.Name;
            contract.ClientAccountUsername = client.AccountUsername;
            contract.StartDate = contractDto.StartDate;
            contract.EndDate = contractDto.EndDate;
            contract.Status = string.IsNullOrWhiteSpace(contractDto.Status) ? "Draft" : contractDto.Status.Trim();
            contract.ServiceLevel = contractDto.ServiceLevel;
            contract.AgreementFileName = contractDto.AgreementFileName;
            contract.ClientReturnedAgreementFileName = contractDto.ClientReturnedAgreementFileName;
            contract.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract updated successfully with ID: {ContractId}", contract.Id);
            return MapToContractDto(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract with ID: {ContractId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ContractDto?> UpdateContractStatusAsync(int id, UpdateContractStatusDto statusDto)
    {
        try
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                _logger.LogWarning("Attempt to update non-existent contract with ID: {ContractId}", id);
                return null;
            }

            contract.Status = statusDto.Status;
            contract.ModifiedDate = DateTime.UtcNow;

            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract status updated successfully for ID: {ContractId} to status: {Status}", id, statusDto.Status);
            return MapToContractDto(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract status for ID: {ContractId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ContractDto?> UpdateReturnedAgreementAsync(int id, string storedFileName)
    {
        try
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                _logger.LogWarning("Attempt to update returned agreement for missing contract with ID: {ContractId}", id);
                return null;
            }

            contract.ClientReturnedAgreementFileName = storedFileName;
            contract.ModifiedDate = DateTime.UtcNow;

            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated returned agreement for contract {ContractId}", id);
            return MapToContractDto(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating returned agreement for ID: {ContractId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteContractAsync(int id)
    {
        try
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                _logger.LogWarning("Attempt to delete non-existent contract with ID: {ContractId}", id);
                return false;
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract deleted successfully with ID: {ContractId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contract with ID: {ContractId}", id);
            throw;
        }
    }

    /// <summary>
    /// Maps a Contract entity to a ContractDto.
    /// </summary>
    private static ContractDto MapToContractDto(Contract contract)
    {
        return new ContractDto
        {
            Id = contract.Id,
            ClientId = contract.ClientId,
            ClientName = contract.Client?.Name ?? contract.ClientName,
            ClientAccountUsername = contract.Client?.AccountUsername ?? contract.ClientAccountUsername,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status,
            ServiceLevel = contract.ServiceLevel,
            AgreementFileName = contract.AgreementFileName,
            ClientReturnedAgreementFileName = contract.ClientReturnedAgreementFileName,
            CreatedDate = contract.CreatedDate,
            ModifiedDate = contract.ModifiedDate
        };
    }

    private async Task<Client?> ResolveClientAsync(ContractDto contractDto)
    {
        if (contractDto.ClientId > 0)
        {
            return await _context.Clients.FindAsync(contractDto.ClientId);
        }

        if (!string.IsNullOrWhiteSpace(contractDto.ClientAccountUsername))
        {
            var accountUsername = contractDto.ClientAccountUsername.Trim();
            return await _context.Clients.FirstOrDefaultAsync(c => c.AccountUsername == accountUsername);
        }

        if (!string.IsNullOrWhiteSpace(contractDto.ClientName))
        {
            var clientName = contractDto.ClientName.Trim();
            return await _context.Clients.FirstOrDefaultAsync(c => c.Name == clientName);
        }

        return null;
    }
}
