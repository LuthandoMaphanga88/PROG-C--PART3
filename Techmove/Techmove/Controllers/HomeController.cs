using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services.Api;

namespace Techmove.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ITechmoveApiClient _apiClient;

        public HomeController(ITechmoveApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var clients = await _apiClient.GetClientsAsync(cancellationToken);
            var contracts = await _apiClient.GetContractsAsync(cancellationToken);
            var serviceRequests = await _apiClient.GetServiceRequestsAsync(cancellationToken);

            var dashboard = new DashboardViewModel
            {
                TotalClients = clients.Count,
                ActiveContracts = contracts.Count(c => c.Status == "Active"),
                OpenServiceRequests = serviceRequests.Count(r => r.Status is "Open" or "Pending Approval" or "In Progress"),
                ExpiringContracts = contracts.Count(c => c.EndDate >= DateTime.Today && c.EndDate <= DateTime.Today.AddDays(30))
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
