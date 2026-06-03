using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Techmove.Services.Api;

public static class JwtTokenFactory
{
    public static string CreateToken(IConfiguration configuration)
    {
        var issuer = configuration["ApiAuthentication:Issuer"] ?? "Techmove.Mvc";
        var audience = configuration["ApiAuthentication:Audience"] ?? "Techmove.Api";
        var signingKey = configuration["ApiAuthentication:SigningKey"]
            ?? throw new InvalidOperationException("ApiAuthentication:SigningKey is not configured.");

        var now = DateTimeOffset.UtcNow;
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object>
        {
            ["iss"] = issuer,
            ["aud"] = audience,
            ["sub"] = "techmove-mvc",
            ["iat"] = now.ToUnixTimeSeconds(),
            ["nbf"] = now.ToUnixTimeSeconds(),
            ["exp"] = now.AddMinutes(30).ToUnixTimeSeconds()
        };

        var unsignedToken = $"{Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header))}.{Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload))}";
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingKey), Encoding.UTF8.GetBytes(unsignedToken));
        return $"{unsignedToken}.{Base64UrlEncode(signature)}";
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
