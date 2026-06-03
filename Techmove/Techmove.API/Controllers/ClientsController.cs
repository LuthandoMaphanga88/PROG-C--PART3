using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Techmove.API.Models;
using Techmove.Data;
using Techmove.Models;

namespace Techmove.API.Controllers;

[ApiController]
[Route("api/clients")]
[Produces("application/json")]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
    {
        var clients = await _context.Clients
            .OrderBy(client => client.Name)
            .Select(client => MapToDto(client))
            .ToListAsync();

        return Ok(clients);
    }

    [HttpGet("by-account/{accountUsername}")]
    public async Task<ActionResult<ClientDto>> GetByAccountUsername(string accountUsername)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.AccountUsername == accountUsername);
        return client is null ? NotFound(new { message = "Client not found." }) : Ok(MapToDto(client));
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClient(ClientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingClient = !string.IsNullOrWhiteSpace(dto.AccountUsername)
            ? await _context.Clients.FirstOrDefaultAsync(c => c.AccountUsername == dto.AccountUsername)
            : null;

        if (existingClient is not null)
        {
            ApplyDto(existingClient, dto);
            await _context.SaveChangesAsync();
            return Ok(MapToDto(existingClient));
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

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByAccountUsername), new { accountUsername = client.AccountUsername }, MapToDto(client));
    }

    [HttpPut("by-account/{accountUsername}")]
    public async Task<ActionResult<ClientDto>> UpsertClientProfile(string accountUsername, ClientDto dto)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.AccountUsername == accountUsername);
        if (client is null)
        {
            client = new Client
            {
                AccountUsername = accountUsername,
                CreatedDate = DateTime.UtcNow
            };
            _context.Clients.Add(client);
        }

        ApplyDto(client, dto);
        client.AccountUsername = accountUsername;
        await _context.SaveChangesAsync();

        return Ok(MapToDto(client));
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

    private static void ApplyDto(Client client, ClientDto dto)
    {
        client.Name = dto.Name;
        client.ContactDetails = dto.ContactDetails;
        client.Region = dto.Region;
        if (!string.IsNullOrWhiteSpace(dto.AccountUsername))
        {
            client.AccountUsername = dto.AccountUsername;
        }
        client.ModifiedDate = DateTime.UtcNow;
    }
}
