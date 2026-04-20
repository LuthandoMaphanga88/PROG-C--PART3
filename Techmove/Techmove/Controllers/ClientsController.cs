using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services;

namespace Techmove.Controllers;

[Authorize(Roles = "Admin")]
public class ClientsController : Controller
{
    private readonly InMemoryDataStore _dataStore;

    public ClientsController(InMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public IActionResult Index()
    {
        return View(_dataStore.Clients);
    }

    public IActionResult Create()
    {
        return View(new ClientViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ClientViewModel client)
    {
        if (!ModelState.IsValid)
        {
            return View(client);
        }

        _dataStore.AddClient(client);
        return RedirectToAction(nameof(Index));
    }
}
