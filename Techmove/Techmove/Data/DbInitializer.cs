using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Techmove.Data;

/// <summary>
/// Helper class for database initialization and management.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database by running pending migrations.
    /// This should be called during application startup.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static void InitializeDatabase(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                var environment = services.GetRequiredService<IWebHostEnvironment>();

                if (environment.IsDevelopment())
                {
                    // Dev-only hard reset to guarantee schema exists (fixes missing-tables like 'Clients').
                    // This is intentionally destructive.
                    var connString = context.Database.GetConnectionString();
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("[DbInitializer] Development DB reset starting. ConnectionString='{ConnectionString}'", connString);

                    context.Database.EnsureDeleted();
                    logger.LogInformation("[DbInitializer] Development DB deleted (if it existed)." );

                    context.Database.EnsureCreated();
                    logger.LogInformation("[DbInitializer] Development DB EnsureCreated completed." );

                    return;
                }

                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
                else if (!context.Database.CanConnect())
                {
                    context.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }
    }
}
