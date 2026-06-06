using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Techmove.API.Authentication;

public class JwtApiAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtApiAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicEndpoint(context))
        {
            await _next(context);
            return;
        }

        // Check for API Key first
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            if (IsValidApiKey(apiKey.ToString()))
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid API key." });
            return;
        }

        // Fall back to Bearer token
        var authorizationHeader = context.Request.Headers.Authorization.ToString();
        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Missing API key or bearer token." });
            return;
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();

        // Allow dev API key to be passed as a bearer token too
        const string devApiKey = "techmove-dev-key-2026";
        if (string.Equals(token, devApiKey, StringComparison.Ordinal))
        {
            await _next(context);
            return;
        }

        if (!IsValidToken(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid bearer token." });
            return;
        }

        await _next(context);
    }

    private static bool IsPublicEndpoint(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/swagger") ||
               context.Request.Path.StartsWithSegments("/openapi") ||
               context.Request.Path.StartsWithSegments("/api/auth/token") ||
               context.Request.Path.StartsWithSegments("/favicon.ico");
    }

    private bool IsValidApiKey(string apiKey)
    {
        // Development API key for testing
        const string devApiKey = "techmove-dev-key-2026";
        return apiKey == devApiKey;
    }

    private bool IsValidToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return false;
        }

        var signingKey = _configuration["ApiAuthentication:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            return false;
        }

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = Base64UrlEncode(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(signingKey),
            Encoding.UTF8.GetBytes(unsignedToken)));

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(expectedSignature),
                Encoding.ASCII.GetBytes(parts[2])))
        {
            return false;
        }

        using var payloadDocument = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var payload = payloadDocument.RootElement;
        var issuer = payload.GetProperty("iss").GetString();
        var audience = payload.GetProperty("aud").GetString();
        var expires = payload.GetProperty("exp").GetInt64();

        return string.Equals(issuer, _configuration["ApiAuthentication:Issuer"], StringComparison.Ordinal) &&
               string.Equals(audience, _configuration["ApiAuthentication:Audience"], StringComparison.Ordinal) &&
               DateTimeOffset.UtcNow.ToUnixTimeSeconds() < expires;
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
