using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Techmove.API.Models;
using Techmove.Data;
using Techmove.Models;

namespace Techmove.API.Controllers;

[ApiController]
[Route("api/service-requests")]
[Produces("application/json")]
public class ServiceRequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServiceRequestsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetServiceRequests()
    {
        var requests = await _context.ServiceRequests
            .OrderByDescending(request => request.CreatedDate)
            .Select(request => MapToDto(request))
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequestDto>> CreateServiceRequest(ServiceRequestDto dto)
    {
        var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == dto.ContractId);
        if (contract is null)
        {
            return BadRequest(new { message = "Please select a valid contract." });
        }

        if (contract.Status is "Expired" or "On Hold")
        {
            return BadRequest(new { message = "A service request cannot be created for Expired or On Hold contracts." });
        }

        var request = new ServiceRequest
        {
            ContractId = contract.Id,
            ContractRef = string.IsNullOrWhiteSpace(dto.ContractRef)
                ? $"CT-{contract.Id} - {contract.ClientName} ({contract.Status})"
                : dto.ContractRef,
            Description = dto.Description,
            CostUsd = dto.CostUsd,
            CostZar = dto.CostZar,
            Status = dto.Status,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetServiceRequests), new { id = request.Id }, MapToDto(request));
    }

    private static ServiceRequestDto MapToDto(ServiceRequest request)
    {
        return new ServiceRequestDto
        {
            Id = request.Id,
            ContractId = request.ContractId,
            ContractRef = request.ContractRef,
            Description = request.Description,
            CostUsd = request.CostUsd,
            CostZar = request.CostZar,
            Status = request.Status
        };
    }
}
