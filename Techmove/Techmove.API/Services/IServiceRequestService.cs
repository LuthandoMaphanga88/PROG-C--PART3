using Techmove.API.Models;

namespace Techmove.API.Services;

/// <summary>
/// Service interface for service request operations.
/// Abstracts business logic from the API controller.
/// </summary>
public interface IServiceRequestService
{
    Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsAsync(string? status = null, int? contractId = null);
    Task<ServiceRequestDto?> GetServiceRequestByIdAsync(int id);
    Task<ServiceRequestDto> CreateServiceRequestAsync(ServiceRequestDto dto);
    Task<ServiceRequestDto?> UpdateServiceRequestAsync(int id, ServiceRequestDto dto);
    Task<bool> DeleteServiceRequestAsync(int id);
}
