namespace Techmove.API.Models;

public sealed class InventoryItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset ReleaseDate { get; set; }
    public ManufacturerDto Manufacturer { get; set; } = new();
}

public sealed class ManufacturerDto
{
    public string Name { get; set; } = string.Empty;
    public string? HomePage { get; set; }
    public string? Phone { get; set; }
}
