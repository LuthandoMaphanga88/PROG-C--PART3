using Microsoft.AspNetCore.Mvc;
using Techmove.API.Models;

namespace Techmove.API.Controllers;

[ApiController]
[Route("inventory")]
public class InventoryController : ControllerBase
{
    private static readonly List<InventoryItemDto> Items =
    [
        new()
        {
            Id = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851"),
            Name = "Widget Adapter",
            ReleaseDate = DateTimeOffset.Parse("2016-08-29T09:12:33.001Z"),
            Manufacturer = new ManufacturerDto
            {
                Name = "ACME Corporation",
                HomePage = "https://www.acme-corp.com",
                Phone = "408-867-5309"
            }
        }
    ];

    [HttpGet]
    [Tags("developers")]
    [ProducesResponseType(typeof(IReadOnlyList<InventoryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyList<InventoryItemDto>> SearchInventory(
        [FromQuery] string? searchString,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        if (skip < 0 || limit is < 0 or > 50)
        {
            return BadRequest();
        }

        var query = Items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(item =>
                item.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                item.Manufacturer.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(query.Skip(skip).Take(limit).ToList());
    }

    [HttpPost]
    [Tags("admins")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult AddInventory([FromBody] InventoryItemDto item)
    {
        if (item.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(item.Name) ||
            string.IsNullOrWhiteSpace(item.Manufacturer.Name))
        {
            return BadRequest();
        }

        if (Items.Any(existing => existing.Id == item.Id))
        {
            return Conflict();
        }

        Items.Add(item);
        return Created($"/inventory/{item.Id}", item);
    }
}
