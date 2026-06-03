using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Techmove.Services;

internal static class DevelopmentHttpClientHandler
{
    public static HttpMessageHandler Create(IServiceProvider serviceProvider)
    {
        var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
        var handler = new HttpClientHandler();

        if (environment.IsDevelopment())
        {
            handler.ServerCertificateCustomValidationCallback = ValidateDevelopmentCertificate;
        }

        return handler;
    }

    private static bool ValidateDevelopmentCertificate(
        HttpRequestMessage? request,
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        var host = request?.RequestUri?.Host;
        if (host is "localhost" or "127.0.0.1")
        {
            return true;
        }

        // Lab networks sometimes intercept outbound HTTPS (e.g. exchange-rate API).
        return true;
    }
}
