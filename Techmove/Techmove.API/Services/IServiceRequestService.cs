namespace Techmove.API.Services;

/// <summary>
/// Service interface for service request operations.
/// Abstracts business logic from the API controller.
/// </summary>
public interface IServiceRequestService
{
    /// <summary>
    /// Get all service requests with optional filtering.
    /// </summary>
    /// <param name="status">Optional filter by status</param>
    /// <param name="contractId">Optional filter by contract ID</param>
    /// <returns>Collection of service request DTOs</returns>
    Task<IEnumerable<object>> GetServiceRequestsAsync(string? status = null, int? contractId = null);

    /// <summary>
    /// Get a specific service request by ID.
    /// </summary>
    /// <param name="id">Service request ID</param>
    /// <returns>Service request details or null if not found</returns>
    Task<object?> GetServiceRequestByIdAsync(int id);

    /// <summary>
    /// Create a new service request.
    /// </summary>
    /// <param name="serviceRequestData">Service request data</param>
    /// <returns>Created service request with ID</returns>
    Task<object> CreateServiceRequestAsync(object serviceRequestData);

    /// <summary>
    /// Update service request status.
    /// </summary>
    /// <param name="id">Service request ID</param>
    /// <param name="statusData">New status data</param>
    /// <returns>Updated service request</returns>
    Task<object?> UpdateServiceRequestStatusAsync(int id, object statusData);

    /// <summary>
    /// Delete a service request.
    /// </summary>
    /// <param name="id">Service request ID</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> DeleteServiceRequestAsync(int id);
}
