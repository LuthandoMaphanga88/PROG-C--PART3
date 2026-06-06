using Microsoft.EntityFrameworkCore;
using Techmove.API.Models;
using Techmove.API.Repositories;
using Techmove.Models;

namespace Techmove.API.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IRepository<ServiceRequest> _serviceRequestRepository;
    private readonly IRepository<Contract> _contractRepository;
    private readonly ILogger<ServiceRequestService> _logger;

    public ServiceRequestService(
        IRepository<ServiceRequest> serviceRequestRepository,
        IRepository<Contract> contractRepository,
        ILogger<ServiceRequestService> logger)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _contractRepository = contractRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsAsync(string? status = null, int? contractId = null)
    {
        var query = _serviceRequestRepository.Query();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(request => request.Status == status);
        }

        if (contractId.HasValue)
        {
            query = query.Where(request => request.ContractId == contractId.Value);
        }

        var requests = await query
            .OrderByDescending(request => request.CreatedDate)
            .AsNoTracking()
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    public async Task<ServiceRequestDto?> GetServiceRequestByIdAsync(int id)
    {
        var request = await _serviceRequestRepository.GetByIdAsync(id);
        return request is null ? null : MapToDto(request);
    }

    public async Task<ServiceRequestDto> CreateServiceRequestAsync(ServiceRequestDto dto)
    {
        var contract = await _contractRepository.GetByIdAsync(dto.ContractId);
        if (contract is null)
        {
            _logger.LogWarning("Attempt to create a service request for an invalid contract ID {ContractId}", dto.ContractId);
            throw new InvalidOperationException("Please select a valid contract.");
        }

        if (contract.Status is "Expired" or "On Hold")
        {
            _logger.LogWarning("Attempt to create a service request for contract {ContractId} with invalid status {Status}", contract.Id, contract.Status);
            throw new InvalidOperationException("A service request cannot be created for Expired or On Hold contracts.");
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

        await _serviceRequestRepository.AddAsync(request);
        await _serviceRequestRepository.SaveChangesAsync();

        return MapToDto(request);
    }

    public async Task<ServiceRequestDto?> UpdateServiceRequestAsync(int id, ServiceRequestDto dto)
    {
        var request = await _serviceRequestRepository.GetByIdAsync(id);
        if (request is null)
        {
            return null;
        }

        var contract = await _contractRepository.GetByIdAsync(dto.ContractId);
        if (contract is null)
        {
            _logger.LogWarning("Attempt to update service request {ServiceRequestId} with invalid contract ID {ContractId}", id, dto.ContractId);
            throw new InvalidOperationException("Please select a valid contract.");
        }

        if (contract.Status is "Expired" or "On Hold")
        {
            _logger.LogWarning("Attempt to update service request {ServiceRequestId} with contract {ContractId} in invalid status {Status}", id, contract.Id, contract.Status);
            throw new InvalidOperationException("A service request cannot be linked to Expired or On Hold contracts.");
        }

        request.ContractId = contract.Id;
        request.ContractRef = string.IsNullOrWhiteSpace(dto.ContractRef)
            ? $"CT-{contract.Id} - {contract.ClientName} ({contract.Status})"
            : dto.ContractRef;
        request.Description = dto.Description;
        request.CostUsd = dto.CostUsd;
        request.CostZar = dto.CostZar;
        request.Status = dto.Status;
        request.ModifiedDate = DateTime.UtcNow;

        await _serviceRequestRepository.UpdateAsync(request);
        await _serviceRequestRepository.SaveChangesAsync();

        return MapToDto(request);
    }

    public async Task<bool> DeleteServiceRequestAsync(int id)
    {
        var request = await _serviceRequestRepository.GetByIdAsync(id);
        if (request is null)
        {
            return false;
        }

        await _serviceRequestRepository.DeleteAsync(request);
        await _serviceRequestRepository.SaveChangesAsync();
        return true;
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
