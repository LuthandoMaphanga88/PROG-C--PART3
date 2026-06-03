using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Techmove.Models;
using Techmove.Services.Api;

namespace Techmove.Controllers;

[Authorize(Roles = "Client")]
public class ClientProfileController : Controller
{
    private readonly ITechmoveApiClient _apiClient;
    private readonly ILogger<ClientProfileController> _logger;

    public ClientProfileController(ITechmoveApiClient apiClient, ILogger<ClientProfileController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Edit(CancellationToken cancellationToken) //Microsoft (2024)
    {
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var existingClient = await _apiClient.GetClientByAccountUsernameAsync(username, cancellationToken);
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
    public async Task<IActionResult> Edit(ClientViewModel model, CancellationToken cancellationToken)
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

        try
        {
            await _apiClient.SaveClientProfileAsync(username, model, cancellationToken);
            TempData["ProfileSaved"] = "Your information has been saved.";
            return RedirectToAction(nameof(Edit));
        }
        catch (TechmoveApiException ex)
        {
            _logger.LogWarning(ex, "Unable to save client profile for {Username}", username);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Unable to contact the Techmove API while saving profile for {Username}", username);
            ModelState.AddModelError(string.Empty, "Your information could not be saved because the API is unavailable. Please try again.");
            return View(model);
        }
    }
}
//Reference list:
//Microsoft (2024) Tutorial: Implement CRUD functionality with ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/validation 
