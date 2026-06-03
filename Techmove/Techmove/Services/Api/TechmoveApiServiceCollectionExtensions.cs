namespace Techmove.Services.Api;

public static class TechmoveApiServiceCollectionExtensions
{
    public static IServiceCollection AddTechmoveApiClient(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSingleton<InMemoryDataStore>();

        var baseUrl = configuration["TechmoveApi:BaseUrl"] ?? "https://localhost:7000/";
        var forceInMemory = configuration.GetValue("TechmoveApi:UseInMemory", false);
        var preferHttp = configuration.GetValue("TechmoveApi:PreferHttp", true);
        var timeoutSeconds = configuration.GetValue("TechmoveApi:TimeoutSeconds", 30);
        var apiTimeout = TimeSpan.FromSeconds(timeoutSeconds);

        var useHttpClient = preferHttp &&
                            !forceInMemory &&
                            TechmoveApiHealthChecker.IsReachable(baseUrl);

        if (useHttpClient)
        {
            services.AddHttpClient<ITechmoveApiClient, TechmoveApiClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = apiTimeout;
            })
            .ConfigurePrimaryHttpMessageHandler(DevelopmentHttpClientHandler.Create);
        }
        else
        {
            services.AddSingleton<ITechmoveApiClient, InMemoryTechmoveApiClient>();
        }

        services.AddSingleton(new TechmoveApiClientMode(useHttpClient ? "Http" : "InMemory", baseUrl));
        return services;
    }
}

public sealed record TechmoveApiClientMode(string Mode, string BaseUrl);
