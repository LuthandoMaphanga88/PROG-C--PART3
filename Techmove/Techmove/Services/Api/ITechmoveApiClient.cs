using Techmove.Models;

namespace Techmove.Services.Api;

public interface ITechmoveApiClient
{
    Task<IReadOnlyList<ClientViewModel>> GetClientsAsync(CancellationToken cancellationToken = default);
    Task<ClientViewModel?> GetClientByAccountUsernameAsync(string accountUsername, CancellationToken cancellationToken = default);
    Task SaveClientAsync(ClientViewModel client, CancellationToken cancellationToken = default);
    Task SaveClientProfileAsync(string accountUsername, ClientViewModel client, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContractViewModel>> GetContractsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContractViewModel>> SearchContractsAsync(string? name, string? client, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<ContractViewModel?> GetContractAsync(int id, CancellationToken cancellationToken = default);
    Task SaveContractAsync(ContractViewModel contract, CancellationToken cancellationToken = default);
    Task SaveReturnedAgreementAsync(int contractId, string storedFileName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRequestViewModel>> GetServiceRequestsAsync(CancellationToken cancellationToken = default);
    Task SaveServiceRequestAsync(ServiceRequestViewModel request, int contractId, CancellationToken cancellationToken = default);
}
