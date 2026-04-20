using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services;

namespace Techmove.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly InMemoryDataStore _dataStore;

        public HomeController(InMemoryDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public IActionResult Index()
        {
            var dashboard = new DashboardViewModel
            {
                TotalClients = _dataStore.Clients.Count,
                ActiveContracts = _dataStore.Contracts.Count(c => c.Status == "Active"),
                OpenServiceRequests = _dataStore.ServiceRequests.Count(r => r.Status is "Open" or "Pending Approval" or "In Progress"),
                ExpiringContracts = _dataStore.Contracts.Count(c => c.EndDate >= DateTime.Today && c.EndDate <= DateTime.Today.AddDays(30))
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
