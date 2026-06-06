using Techmove.Services;
using Techmove.Services.Api;

namespace Techmove
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<Services.InMemoryUserStore>();

            var apiTimeoutSeconds = builder.Configuration.GetValue("TechmoveApi:TimeoutSeconds", 30);
            var apiTimeout = TimeSpan.FromSeconds(apiTimeoutSeconds);

            builder.Services.AddHttpClient<Services.IExchangeRateService, Services.ExchangeRateService>(client =>
            {
                client.Timeout = apiTimeout;
            })
                .ConfigurePrimaryHttpMessageHandler(DevelopmentHttpClientHandler.Create);

            builder.Services.AddTechmoveApiClient(builder.Configuration, builder.Environment);

            builder.Services
                .AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            var apiClientMode = app.Services.GetRequiredService<TechmoveApiClientMode>();
            app.Logger.LogInformation(
                "Techmove data source: {Mode} ({BaseUrl})",
                apiClientMode.Mode,
                apiClientMode.BaseUrl);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            var staticAssetsManifestPath = Path.Combine(app.Environment.WebRootPath ?? "", "..", "Techmove.staticwebassets.endpoints.json");
            if (File.Exists(staticAssetsManifestPath))
            {
                app.MapStaticAssets();
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();
            }
            else
            {
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            }

            app.Run();
        }
    }
}
