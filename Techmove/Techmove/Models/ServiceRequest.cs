namespace Techmove.Models;

/// <summary>
/// Represents a service request in the database.
/// </summary>
public class ServiceRequest
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string ContractRef { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CostUsd { get; set; }
    public decimal CostZar { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    // Foreign key
    public Contract? Contract { get; set; }
}
