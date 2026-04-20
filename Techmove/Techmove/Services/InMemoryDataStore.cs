using Techmove.Models;

namespace Techmove.Services;

public class InMemoryDataStore
{
    private readonly List<ClientViewModel> _clients = [];
    private readonly List<ContractViewModel> _contracts = [];
    private readonly List<ServiceRequestViewModel> _serviceRequests = [];
    private int _nextClientId = 1;
    private int _nextContractId = 100;
    private int _nextServiceRequestId = 500;

    public IReadOnlyList<ClientViewModel> Clients => _clients;
    public IReadOnlyList<ContractViewModel> Contracts => _contracts;
    public IReadOnlyList<ServiceRequestViewModel> ServiceRequests => _serviceRequests;

    public void AddClient(ClientViewModel client)
    {
        if (!string.IsNullOrWhiteSpace(client.AccountUsername))
        {
            var existingByAccount = GetClientByAccountUsername(client.AccountUsername);
            if (existingByAccount is not null)
            {
                existingByAccount.Name = client.Name;
                existingByAccount.ContactDetails = client.ContactDetails;
                existingByAccount.Region = client.Region;
                return;
            }
        }

        client.Id = _nextClientId++;
        _clients.Add(client);
    }

    public ClientViewModel? GetClientByAccountUsername(string accountUsername)
    {
        return _clients.FirstOrDefault(client =>
            string.Equals(client.AccountUsername, accountUsername, StringComparison.OrdinalIgnoreCase));
    }

    public void UpsertClientProfile(string accountUsername, ClientViewModel clientProfile)
    {
        var existingClient = GetClientByAccountUsername(accountUsername);
        if (existingClient is null)
        {
            clientProfile.AccountUsername = accountUsername;
            AddClient(clientProfile);
            return;
        }

        existingClient.Name = clientProfile.Name;
        existingClient.ContactDetails = clientProfile.ContactDetails;
        existingClient.Region = clientProfile.Region;
    }

    public void AddContract(ContractViewModel contract)
    {
        contract.Id = _nextContractId++;
        _contracts.Add(contract);
    }

    public IReadOnlyList<ContractViewModel> GetContractsByClientAccountUsername(string accountUsername)
    {
        return _contracts
            .Where(contract => string.Equals(contract.ClientAccountUsername, accountUsername, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public ContractViewModel? GetContractById(int contractId)
    {
        return _contracts.FirstOrDefault(contract => contract.Id == contractId);
    }

    public void AddServiceRequest(ServiceRequestViewModel request)
    {
        request.Id = _nextServiceRequestId++;
        _serviceRequests.Add(request);
    }
}
