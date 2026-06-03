using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services.Api;

namespace Techmove.Controllers;

[Authorize(Roles = "Admin")]
public class ClientsController : Controller
{
    private readonly ITechmoveApiClient _apiClient;

    public ClientsController(ITechmoveApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var clients = await _apiClient.GetClientsAsync(cancellationToken);
        return View(clients);
    }

    public IActionResult Create() //Microsoft (2024)
    {
        return View(new ClientViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientViewModel client, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(client);
        }

        await _apiClient.SaveClientAsync(client, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
//References:
//Microsoft (2024) Tutorial: Implement CRUD functionality with ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/validation 
