namespace Techmove.API.Models;

/// <summary>
/// Data transfer object for Contract API responses and requests.
/// </summary>
public class ContractDto
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
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
