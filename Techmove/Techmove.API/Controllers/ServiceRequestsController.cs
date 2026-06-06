using Microsoft.AspNetCore.Mvc;
using Techmove.API.Models;
using Techmove.API.Services;

namespace Techmove.API.Controllers;

[ApiController]
[Route("api/service-requests")]
[Produces("application/json")]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;

    public ServiceRequestsController(IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetServiceRequests()
    {
        var requests = await _serviceRequestService.GetServiceRequestsAsync();
        return Ok(requests);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceRequestDto>> GetServiceRequest(int id)
    {
        var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
        return request is null ? NotFound(new { message = "Service request not found." }) : Ok(request);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequestDto>> CreateServiceRequest(ServiceRequestDto dto)
    {
        try
        {
            var request = await _serviceRequestService.CreateServiceRequestAsync(dto);
            return CreatedAtAction(nameof(GetServiceRequest), new { id = request.Id }, request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceRequestDto>> UpdateServiceRequest(int id, ServiceRequestDto dto)
    {
        try
        {
            var request = await _serviceRequestService.UpdateServiceRequestAsync(id, dto);
            return request is null ? NotFound(new { message = "Service request not found." }) : Ok(request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteServiceRequest(int id)
    {
        var deleted = await _serviceRequestService.DeleteServiceRequestAsync(id);
        return deleted ? NoContent() : NotFound(new { message = "Service request not found." });
    }
}
