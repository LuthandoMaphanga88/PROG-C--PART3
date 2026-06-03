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

    public ClientViewModel? GetClientById(int id) =>
        _clients.FirstOrDefault(client => client.Id == id);

    public ClientViewModel? GetClientByAccountUsername(string accountUsername)
    {
        return _clients.FirstOrDefault(client =>
            string.Equals(client.AccountUsername, accountUsername, StringComparison.OrdinalIgnoreCase));
    }

    public void UpdateClient(int id, ClientViewModel client)
    {
        var existingClient = GetClientById(id)
            ?? throw new InvalidOperationException("Client not found.");

        existingClient.AccountUsername = client.AccountUsername;
        existingClient.Name = client.Name;
        existingClient.ContactDetails = client.ContactDetails;
        existingClient.Region = client.Region;
    }

    public bool DeleteClient(int id)
    {
        var existingClient = GetClientById(id);
        if (existingClient is null)
        {
            return false;
        }

        if (_contracts.Any(contract => contract.ClientName == existingClient.Name))
        {
            throw new InvalidOperationException("This client cannot be deleted while related contracts exist.");
        }

        return _clients.Remove(existingClient);
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

    public bool UpdateContract(int id, ContractViewModel contract)
    {
        var existingContract = GetContractById(id);
        if (existingContract is null)
        {
            return false;
        }

        existingContract.ClientName = contract.ClientName;
        existingContract.ClientAccountUsername = contract.ClientAccountUsername;
        existingContract.StartDate = contract.StartDate;
        existingContract.EndDate = contract.EndDate;
        existingContract.Status = contract.Status;
        existingContract.ServiceLevel = contract.ServiceLevel;
        existingContract.AgreementFileName = contract.AgreementFileName;
        existingContract.ClientReturnedAgreementFileName = contract.ClientReturnedAgreementFileName;
        return true;
    }

    public bool DeleteContract(int id)
    {
        var existingContract = GetContractById(id);
        if (existingContract is null)
        {
            return false;
        }

        if (_serviceRequests.Any(request => request.ContractId == id))
        {
            throw new InvalidOperationException("This contract cannot be deleted while related service requests exist.");
        }

        return _contracts.Remove(existingContract);
    }

    public bool SaveReturnedAgreement(int contractId, string storedFileName)
    {
        var existingContract = GetContractById(contractId);
        if (existingContract is null)
        {
            return false;
        }

        existingContract.ClientReturnedAgreementFileName = storedFileName;
        return true;
    }

    public IReadOnlyList<ContractViewModel> SearchContracts(
        string? name,
        string? client,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = _contracts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalizedName = name.Trim();
            query = query.Where(contract =>
                contract.ClientName.Contains(normalizedName, StringComparison.OrdinalIgnoreCase) ||
                contract.ServiceLevel.Contains(normalizedName, StringComparison.OrdinalIgnoreCase) ||
                $"CT-{contract.Id}".Contains(normalizedName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(client))
        {
            query = query.Where(contract => string.Equals(contract.ClientName, client, StringComparison.OrdinalIgnoreCase));
        }

        if (startDate.HasValue)
        {
            query = query.Where(contract => contract.StartDate.Date >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(contract => contract.EndDate.Date <= endDate.Value.Date);
        }

        return query
            .OrderByDescending(contract => contract.StartDate)
            .ThenBy(contract => contract.ClientName)
            .ToList();
    }

    public ServiceRequestViewModel? GetServiceRequestById(int id) =>
        _serviceRequests.FirstOrDefault(request => request.Id == id);

    public bool UpdateServiceRequest(int id, ServiceRequestViewModel request)
    {
        var existingRequest = GetServiceRequestById(id);
        if (existingRequest is null)
        {
            return false;
        }

        existingRequest.ContractId = request.ContractId;
        existingRequest.ContractRef = request.ContractRef;
        existingRequest.Description = request.Description;
        existingRequest.CostUsd = request.CostUsd;
        existingRequest.CostZar = request.CostZar;
        existingRequest.Status = request.Status;
        return true;
    }

    public bool DeleteServiceRequest(int id)
    {
        var existingRequest = GetServiceRequestById(id);
        return existingRequest is not null && _serviceRequests.Remove(existingRequest);
    }

    public void AddServiceRequest(ServiceRequestViewModel request)
    {
        request.Id = _nextServiceRequestId++;
        _serviceRequests.Add(request);
    }
}
