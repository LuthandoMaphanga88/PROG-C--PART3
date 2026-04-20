namespace Techmove.Models;

public class ContractViewModel
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientAccountUsername { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceLevel { get; set; } = string.Empty;
    public string AgreementFileName { get; set; } = string.Empty;
    public string ClientReturnedAgreementFileName { get; set; } = string.Empty;
}
