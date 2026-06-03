using Microsoft.AspNetCore.Http;
using Techmove.Models;

namespace Techmove.Services.Api;

public class InMemoryTechmoveApiClient : ITechmoveApiClient
{
    private readonly InMemoryDataStore _dataStore;

    public InMemoryTechmoveApiClient(InMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<IReadOnlyList<ClientViewModel>> GetClientsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_dataStore.Clients);

    public Task<ClientViewModel?> GetClientAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_dataStore.GetClientById(id));

    public Task<ClientViewModel?> GetClientByAccountUsernameAsync(string accountUsername, CancellationToken cancellationToken = default) =>
        Task.FromResult(_dataStore.GetClientByAccountUsername(accountUsername));

    public Task SaveClientAsync(ClientViewModel client, CancellationToken cancellationToken = default)
    {
        _dataStore.AddClient(client);
        return Task.CompletedTask;
    }

    public Task UpdateClientAsync(int id, ClientViewModel client, CancellationToken cancellationToken = default)
    {
        _dataStore.UpdateClient(id, client);
        return Task.CompletedTask;
    }

    public Task DeleteClientAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_dataStore.DeleteClient(id))
            {
                throw new TechmoveApiException("Client not found.", StatusCodes.Status404NotFound);
            }
        }
        catch (InvalidOperationException ex)
        {
            throw new TechmoveApiException(ex.Message, StatusCodes.Status400BadRequest);
        }

        return Task.CompletedTask;
    }

    public Task SaveClientProfileAsync(string accountUsername, ClientViewModel client, CancellationToken cancellationToken = default)
    {
        _dataStore.UpsertClientProfile(accountUsername, client);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ContractViewModel>> GetContractsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_dataStore.Contracts);

    public Task<IReadOnlyList<ContractViewModel>> SearchContractsAsync(
        string? name,
        string? client,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_dataStore.SearchContracts(name, client, startDate, endDate));

    public Task<ContractViewModel?> GetContractAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_dataStore.GetContractById(id));

    public Task SaveContractAsync(ContractViewModel contract, CancellationToken cancellationToken = default)
    {
        _dataStore.AddContract(contract);
        return Task.CompletedTask;
    }

    public Task UpdateContractAsync(int id, ContractViewModel contract, CancellationToken cancellationToken = default)
    {
        if (!_dataStore.UpdateContract(id, contract))
        {
            throw new TechmoveApiException("Contract not found.", StatusCodes.Status404NotFound);
        }

        return Task.CompletedTask;
    }

    public Task DeleteContractAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_dataStore.DeleteContract(id))
        {
            throw new TechmoveApiException("Contract not found.", StatusCodes.Status404NotFound);
        }

        return Task.CompletedTask;
    }

    public Task SaveReturnedAgreementAsync(int contractId, string storedFileName, CancellationToken cancellationToken = default)
    {
        if (!_dataStore.SaveReturnedAgreement(contractId, storedFileName))
        {
            throw new TechmoveApiException("Contract not found.", StatusCodes.Status404NotFound);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ServiceRequestViewModel>> GetServiceRequestsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_dataStore.ServiceRequests);

    public Task<ServiceRequestViewModel?> GetServiceRequestAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_dataStore.GetServiceRequestById(id));

    public Task SaveServiceRequestAsync(ServiceRequestViewModel request, int contractId, CancellationToken cancellationToken = default)
    {
        request.ContractId = contractId;
        _dataStore.AddServiceRequest(request);
        return Task.CompletedTask;
    }

    public Task UpdateServiceRequestAsync(int id, ServiceRequestViewModel request, CancellationToken cancellationToken = default)
    {
        if (!_dataStore.UpdateServiceRequest(id, request))
        {
            throw new TechmoveApiException("Service request not found.", StatusCodes.Status404NotFound);
        }

        return Task.CompletedTask;
    }

    public Task DeleteServiceRequestAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_dataStore.DeleteServiceRequest(id))
        {
            throw new TechmoveApiException("Service request not found.", StatusCodes.Status404NotFound);
        }

        return Task.CompletedTask;
    }
}
