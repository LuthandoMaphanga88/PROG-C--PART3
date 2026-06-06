using Techmove.API.Models;

namespace Techmove.API.Services;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetClientsAsync();
    Task<ClientDto?> GetClientByIdAsync(int id);
    Task<ClientDto?> GetClientByAccountUsernameAsync(string accountUsername);
    Task<ClientDto> CreateClientAsync(ClientDto dto);
    Task<ClientDto> UpsertClientProfileAsync(string accountUsername, ClientDto dto);
    Task<ClientDto?> UpdateClientAsync(int id, ClientDto dto);
    Task<bool> DeleteClientAsync(int id);
}
