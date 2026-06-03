using Microsoft.AspNetCore.Mvc;
using Techmove.API.Services;

namespace Techmove.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetAll()
    {
        var contracts = await _contractService.GetAllContractsAsync();
        return Ok(contracts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<dynamic>> GetById(int id)
    {
        var contract = await _contractService.GetContractByIdAsync(id);
        if (contract == null)
            return NotFound();
        return Ok(contract);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] dynamic contract)
    {
        await _contractService.CreateContractAsync(contract);
        return CreatedAtAction(nameof(GetById), contract);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] dynamic contract)
    {
        await _contractService.UpdateContractAsync(id, contract);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _contractService.DeleteContractAsync(id);
        return NoContent();
    }
}
