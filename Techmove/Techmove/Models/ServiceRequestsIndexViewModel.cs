namespace Techmove.Models;

public class ServiceRequestsIndexViewModel
{
    public IReadOnlyList<ServiceRequestViewModel> Requests { get; set; } = [];
    public decimal EurRate { get; set; }
    public decimal GbpRate { get; set; }
}
