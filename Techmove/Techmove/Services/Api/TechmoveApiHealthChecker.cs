using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Techmove.Services.Api;

internal static class TechmoveApiHealthChecker
{
    public static bool IsReachable(string baseUrl, int timeoutSeconds = 3)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return false;
        }

        try
        {
            using var handler = new HttpClientHandler();
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) &&
                baseUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
                baseUri.Host is "localhost" or "127.0.0.1")
            {
                handler.ServerCertificateCustomValidationCallback = AcceptLocalhostCertificate;
            }

            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };

            var healthUri = new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), "swagger/index.html");
            using var response = client.GetAsync(healthUri).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static bool AcceptLocalhostCertificate(
        HttpRequestMessage? request,
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors) => true;
}
