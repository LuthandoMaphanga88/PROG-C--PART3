using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services.Api;

namespace Techmove.Controllers;

[Authorize(Roles = "Admin")]
public class ClientsController : Controller
{
    private readonly ITechmoveApiClient _apiClient;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(ITechmoveApiClient apiClient, ILogger<ClientsController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var clients = await _apiClient.GetClientsAsync(cancellationToken);
            return View(clients);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to load clients from the Techmove API");
            TempData["ClientError"] = ex.Message;
            return View(Array.Empty<ClientViewModel>());
        }
    }

    public IActionResult Create() //Microsoft (2024)
    {
        return View(new ClientViewModel());
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var client = await _apiClient.GetClientAsync(id, cancellationToken);
        return client is null ? NotFound() : View(client);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientViewModel client, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(client);
        }

        try
        {
            await _apiClient.SaveClientAsync(client, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to save client");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(client);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while saving client");
            ModelState.AddModelError(
                string.Empty,
                "The client could not be saved because the API is unavailable. Start Techmove.API (https://localhost:7000), or restart the MVC app to use in-memory fallback.");
            return View(client);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(client);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var client = await _apiClient.GetClientAsync(id, cancellationToken);
        return client is null ? NotFound() : View(client);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientViewModel client, CancellationToken cancellationToken)
    {
        if (id != client.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(client);
        }

        try
        {
            await _apiClient.UpdateClientAsync(id, client, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to update client {ClientId}", id);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(client);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while updating client {ClientId}", id);
            ModelState.AddModelError(string.Empty, "The client could not be updated because the API is unavailable. Please try again.");
            return View(client);
        }
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var client = await _apiClient.GetClientAsync(id, cancellationToken);
        return client is null ? NotFound() : View(client);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteClientAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to delete client {ClientId}", id);
            TempData["ClientError"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while deleting client {ClientId}", id);
            TempData["ClientError"] = "The client could not be deleted because the API is unavailable. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }
}
//References:
//Microsoft (2024) Tutorial: Implement CRUD functionality with ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/validation 
