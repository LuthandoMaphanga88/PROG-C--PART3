using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services;

namespace Techmove.Controllers;

[Authorize(Roles = "Client")]
public class ClientProfileController : Controller
{
    private readonly InMemoryDataStore _dataStore;

    public ClientProfileController(InMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public IActionResult Edit()
    {
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var existingClient = _dataStore.GetClientByAccountUsername(username);
        var model = existingClient is null
            ? new ClientViewModel()
            : new ClientViewModel
            {
                Id = existingClient.Id,
                AccountUsername = existingClient.AccountUsername,
                Name = existingClient.Name,
                ContactDetails = existingClient.ContactDetails,
                Region = existingClient.Region
            };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ClientViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(username))
        {
            return RedirectToAction("Login", "Account");
        }

        _dataStore.UpsertClientProfile(username, model);
        TempData["ProfileSaved"] = "Your information has been saved.";
        return RedirectToAction(nameof(Edit));
    }
}
