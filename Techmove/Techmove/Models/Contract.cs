namespace Techmove.Models;

/// <summary>
/// Represents a contract in the database.
/// </summary>
public class Contract
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientAccountUsername { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceLevel { get; set; } = string.Empty;
    public string AgreementFileName { get; set; } = string.Empty;
    public string ClientReturnedAgreementFileName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    // Foreign key
    public Client? Client { get; set; }

    // Navigation property
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = [];
}
