using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Techmove.API.Models;
using Techmove.API.Services;
using Techmove.Data;
using Techmove.Models;

namespace Techmove.API.Controllers;

[ApiController]
[Route("api/clients")]
[Produces("application/json")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
    {
        var clients = await _clientService.GetClientsAsync();
        return Ok(clients);
    }

    [HttpGet("by-account/{accountUsername}")]
    public async Task<ActionResult<ClientDto>> GetByAccountUsername(string accountUsername)
    {
        var client = await _clientService.GetClientByAccountUsernameAsync(accountUsername);
        return client is null ? NotFound(new { message = "Client not found." }) : Ok(client);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientDto>> GetClient(int id)
    {
        var client = await _clientService.GetClientByIdAsync(id);
        return client is null ? NotFound(new { message = "Client not found." }) : Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClient(ClientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var client = await _clientService.CreateClientAsync(dto);
        return CreatedAtAction(nameof(GetByAccountUsername), new { accountUsername = client.AccountUsername }, client);
    }

    [HttpPut("by-account/{accountUsername}")]
    public async Task<ActionResult<ClientDto>> UpsertClientProfile(string accountUsername, ClientDto dto)
    {
        var client = await _clientService.UpsertClientProfileAsync(accountUsername, dto);
        return Ok(client);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClientDto>> UpdateClient(int id, ClientDto dto)
    {
        var client = await _clientService.UpdateClientAsync(id, dto);
        return client is null ? NotFound(new { message = "Client not found." }) : Ok(client);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        try
        {
            var deleted = await _clientService.DeleteClientAsync(id);
            return deleted ? NoContent() : NotFound(new { message = "Client not found." });
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "This client cannot be deleted while related contracts exist." });
        }
    }
}
