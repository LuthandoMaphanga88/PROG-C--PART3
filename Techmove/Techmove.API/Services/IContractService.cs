using Techmove.API.Models;

namespace Techmove.API.Services;

/// <summary>
/// Service interface for contract operations.
/// Abstracts business logic from the API controller.
/// </summary>
public interface IContractService
{
    /// <summary>
    /// Get all contracts with optional filtering.
    /// </summary>
    /// <param name="status">Optional filter by contract status</param>
    /// <param name="clientId">Optional filter by client ID</param>
    /// <returns>Collection of contracts</returns>
    Task<IEnumerable<ContractDto>> GetContractsAsync(
        string? status = null,
        int? clientId = null,
        string? name = null,
        string? client = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Get a specific contract by ID.
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <returns>Contract details or null if not found</returns>
    Task<ContractDto?> GetContractByIdAsync(int id);

    /// <summary>
    /// Create a new contract.
    /// </summary>
    /// <param name="contractDto">Contract data</param>
    /// <returns>Created contract with ID</returns>
    Task<ContractDto> CreateContractAsync(ContractDto contractDto);

    /// <summary>
    /// Update contract status (approve/decline).
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <param name="statusDto">New status</param>
    /// <returns>Updated contract</returns>
    Task<ContractDto?> UpdateContractStatusAsync(int id, UpdateContractStatusDto statusDto);

    /// <summary>
    /// Store the agreement returned by a client.
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <param name="storedFileName">Stored file name</param>
    /// <returns>Updated contract</returns>
    Task<ContractDto?> UpdateReturnedAgreementAsync(int id, string storedFileName);

    /// <summary>
    /// Delete a contract.
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> DeleteContractAsync(int id);
}
