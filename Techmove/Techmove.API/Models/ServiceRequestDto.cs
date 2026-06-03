namespace Techmove.API.Models;

public class ServiceRequestDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string ContractRef { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CostUsd { get; set; }
    public decimal CostZar { get; set; }
    public string Status { get; set; } = string.Empty;
}
