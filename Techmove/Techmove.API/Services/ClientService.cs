using Microsoft.EntityFrameworkCore;
using Techmove.API.Models;
using Techmove.API.Repositories;
using Techmove.Models;

namespace Techmove.API.Services;

public class ClientService : IClientService
{
    private readonly IRepository<Client> _clientRepository;
    private readonly ILogger<ClientService> _logger;

    public ClientService(IRepository<Client> clientRepository, ILogger<ClientService> logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ClientDto>> GetClientsAsync()
    {
        var clients = await _clientRepository.Query()
            .OrderBy(client => client.Name)
            .AsNoTracking()
            .ToListAsync();

        return clients.Select(MapToDto);
    }

    public async Task<ClientDto?> GetClientByIdAsync(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        return client is not null ? MapToDto(client) : null;
    }

    public async Task<ClientDto?> GetClientByAccountUsernameAsync(string accountUsername)
    {
        var client = await _clientRepository.Query()
            .FirstOrDefaultAsync(c => c.AccountUsername == accountUsername);

        return client is not null ? MapToDto(client) : null;
    }

    public async Task<ClientDto> CreateClientAsync(ClientDto dto)
    {
        var existingClient = await _clientRepository.Query()
            .FirstOrDefaultAsync(c => c.AccountUsername == dto.AccountUsername);

        if (existingClient is not null)
        {
            ApplyDto(existingClient, dto, allowAccountUsernameUpdate: false);
            await _clientRepository.SaveChangesAsync();
            return MapToDto(existingClient);
        }

        var client = new Client
        {
            AccountUsername = dto.AccountUsername,
            Name = dto.Name,
            ContactDetails = dto.ContactDetails,
            Region = dto.Region,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();

        return MapToDto(client);
    }

    public async Task<ClientDto> UpsertClientProfileAsync(string accountUsername, ClientDto dto)
    {
        var client = await _clientRepository.Query()
            .FirstOrDefaultAsync(c => c.AccountUsername == accountUsername);

        if (client is null)
        {
            client = new Client
            {
                AccountUsername = accountUsername,
                CreatedDate = DateTime.UtcNow
            };
            await _clientRepository.AddAsync(client);
        }

        ApplyDto(client, dto, allowAccountUsernameUpdate: false);
        client.AccountUsername = accountUsername;
        await _clientRepository.SaveChangesAsync();

        return MapToDto(client);
    }

    public async Task<ClientDto?> UpdateClientAsync(int id, ClientDto dto)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client is null)
        {
            return null;
        }

        ApplyDto(client, dto, allowAccountUsernameUpdate: true);
        await _clientRepository.SaveChangesAsync();

        return MapToDto(client);
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client is null)
        {
            return false;
        }

        await _clientRepository.DeleteAsync(client);
        await _clientRepository.SaveChangesAsync();
        return true;
    }

    private static ClientDto MapToDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            AccountUsername = client.AccountUsername,
            Name = client.Name,
            ContactDetails = client.ContactDetails,
            Region = client.Region
        };
    }

    private static void ApplyDto(Client client, ClientDto dto, bool allowAccountUsernameUpdate)
    {
        if (allowAccountUsernameUpdate && !string.IsNullOrWhiteSpace(dto.AccountUsername))
        {
            client.AccountUsername = dto.AccountUsername;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            client.Name = dto.Name;
        }

        if (!string.IsNullOrWhiteSpace(dto.ContactDetails))
        {
            client.ContactDetails = dto.ContactDetails;
        }

        if (!string.IsNullOrWhiteSpace(dto.Region))
        {
            client.Region = dto.Region;
        }

        client.ModifiedDate = DateTime.UtcNow;
    }
}
