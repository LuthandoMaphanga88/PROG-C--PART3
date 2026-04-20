namespace Techmove.Models;

public class ContractSearchViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public IReadOnlyList<string> ClientOptions { get; set; } = [];
    public IReadOnlyList<ContractViewModel> Results { get; set; } = [];
}
