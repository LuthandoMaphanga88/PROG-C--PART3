using Microsoft.AspNetCore.Mvc;
using Techmove.API.Models;
using Techmove.API.Services;

namespace Techmove.API.Controllers;

/// <summary>
/// API controller for managing contracts.
/// Provides endpoints for retrieving, creating, and updating contracts.
/// </summary>
[ApiController]
[Route("api/contracts")]
[Produces("application/json")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(IContractService contractService, ILogger<ContractsController> logger)
    {
        _contractService = contractService;
        _logger = logger;
    }

    /// <summary>
    /// Get all contracts with optional filtering.
    /// </summary>
    /// <param name="status">Optional: Filter by contract status</param>
    /// <param name="clientId">Optional: Filter by client ID</param>
    /// <param name="name">Optional: Search by client, service level, or contract reference</param>
    /// <param name="client">Optional: Filter by client name</param>
    /// <param name="startDate">Optional: Filter contracts starting on or after this date</param>
    /// <param name="endDate">Optional: Filter contracts ending on or before this date</param>
    /// <returns>List of contracts</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ContractDto>>> GetContracts(
        [FromQuery] string? status = null,
        [FromQuery] int? clientId = null,
        [FromQuery] string? name = null,
        [FromQuery] string? client = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var contracts = await _contractService.GetContractsAsync(
                status,
                clientId,
                name,
                client,
                startDate,
                endDate);

            _logger.LogInformation("Retrieved contracts from API");
            return Ok(contracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contracts");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving contracts" });
        }
    }

    /// <summary>
    /// Get a specific contract by ID.
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <returns>Contract details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractDto>> GetContract(int id)
    {
        try
        {
            var contract = await _contractService.GetContractByIdAsync(id);

            if (contract == null)
            {
                _logger.LogWarning("Contract with ID {ContractId} not found", id);
                return NotFound(new { message = "Contract not found" });
            }

            return Ok(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract {ContractId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the contract" });
        }
    }

    /// <summary>
    /// Create a new contract.
    /// </summary>
    /// <param name="contractDto">Contract data</param>
    /// <returns>Created contract with ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContractDto>> CreateContract([FromBody] ContractDto contractDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (contractDto.StartDate == default ||
                contractDto.EndDate == default ||
                contractDto.EndDate < contractDto.StartDate)
            {
                return BadRequest(new { message = "End date must be the same as or after start date." });
            }

            var contract = await _contractService.CreateContractAsync(contractDto);

            _logger.LogInformation("Created new contract with ID {ContractId}", contract.Id);
            return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contract);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Contract creation validation failed");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contract");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while creating the contract" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractDto>> UpdateContract(int id, [FromBody] ContractDto contractDto)
    {
        try
        {
            if (contractDto.StartDate == default ||
                contractDto.EndDate == default ||
                contractDto.EndDate < contractDto.StartDate)
            {
                return BadRequest(new { message = "End date must be the same as or after start date." });
            }

            var contract = await _contractService.UpdateContractAsync(id, contractDto);

            if (contract is null)
            {
                _logger.LogWarning("Contract with ID {ContractId} not found for update", id);
                return NotFound(new { message = "Contract not found" });
            }

            return Ok(contract);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Contract update validation failed for {ContractId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract {ContractId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the contract" });
        }
    }

    /// <summary>
    /// Update contract status (approve or decline).
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <param name="updateDto">Status update data</param>
    /// <returns>Updated contract</returns>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContractDto>> UpdateContractStatus(int id, [FromBody] UpdateContractStatusDto updateDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(updateDto.Status))
            {
                return BadRequest(new { message = "Status is required" });
            }

            var contract = await _contractService.UpdateContractStatusAsync(id, updateDto);

            if (contract == null)
            {
                _logger.LogWarning("Contract with ID {ContractId} not found for status update", id);
                return NotFound(new { message = "Contract not found" });
            }

            _logger.LogInformation("Updated contract {ContractId} status to {Status}", id, updateDto.Status);
            return Ok(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract status for {ContractId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the contract status" });
        }
    }

    /// <summary>
    /// Store the agreement file returned by a client.
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <param name="returnedAgreement">Returned agreement file metadata</param>
    /// <returns>Updated contract</returns>
    [HttpPatch("{id}/returned-agreement")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContractDto>> UpdateReturnedAgreement(int id, [FromBody] ReturnedAgreementDto returnedAgreement)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(returnedAgreement.StoredFileName))
            {
                return BadRequest(new { message = "Stored file name is required." });
            }

            var contract = await _contractService.UpdateReturnedAgreementAsync(id, returnedAgreement.StoredFileName);

            if (contract == null)
            {
                _logger.LogWarning("Contract with ID {ContractId} not found for returned agreement update", id);
                return NotFound(new { message = "Contract not found" });
            }

            return Ok(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating returned agreement for {ContractId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the returned agreement" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContract(int id)
    {
        try
        {
            var deleted = await _contractService.DeleteContractAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = "Contract not found" });
            }

            return NoContent();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Contract {ContractId} could not be deleted because related data exists", id);
            return BadRequest(new { message = "This contract cannot be deleted while related service requests exist." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contract {ContractId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the contract" });
        }
    }
}
