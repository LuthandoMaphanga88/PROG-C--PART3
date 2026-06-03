namespace Techmove.API.DTOs;

/// <summary>
/// DTO for creating a new contract
/// </summary>
public class CreateContractDto
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientAccountUsername { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string ServiceLevel { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating contract status
/// </summary>
public class UpdateContractStatusDto
{
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO for contract response
/// </summary>
public class ContractResponseDto
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

/// <summary>
/// DTO for contract filtering
/// </summary>
public class ContractFilterDto
{
    public string? Name { get; set; }
    public string? Client { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
