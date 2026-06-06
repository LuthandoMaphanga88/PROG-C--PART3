using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Techmove.API.Authentication;
using Techmove.API.Repositories;
using Techmove.API.Services;
using Techmove.Data;
using Techmove.Services.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Techmove Contracts API",
        Version = "v1",
        Description = "JSON API for managing Techmove contracts."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste a JWT token from /api/auth/token to test secured endpoints."
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter your API key: techmove-dev-key-2026"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvcApp", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

try
{
    app.InitializeDatabase();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database initialization failed.");

    if (!app.Environment.IsDevelopment())
    {
        throw;
    }

    logger.LogWarning("Continuing startup in Development without a ready database.");
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (exceptionHandlerFeature?.Error is not null)
        {
            logger.LogError(
                exceptionHandlerFeature.Error,
                "Unhandled API exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem(
            title: "An unexpected error occurred.",
            detail: "Please try again. If the problem continues, contact support with the request ID.",
            statusCode: StatusCodes.Status500InternalServerError,
            extensions: new Dictionary<string, object?>
            {
                ["requestId"] = context.TraceIdentifier
            })
            .ExecuteAsync(context);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Techmove Contracts API v1");
        options.SwaggerEndpoint("/openapi/simple-inventory.json", "Simple Inventory API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowMvcApp");
app.UseMiddleware<JwtApiAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();

    app.MapGet("/api/auth/token", (IConfiguration configuration) =>
        Results.Ok(new
        {
            token = JwtTokenFactory.CreateToken(configuration),
            tokenType = "Bearer",
            expiresInMinutes = 30
        }))
        .WithTags("Authentication")
        .WithSummary("Generates a development JWT for Swagger testing.")
        .WithDescription("Use this development-only token with the Swagger Authorize button to test secured API endpoints.");

    app.MapGet("/openapi/simple-inventory.json", (IWebHostEnvironment environment) =>
    {
        var specPath = Path.Combine(environment.ContentRootPath, "OpenApi", "simple-inventory-api.json");
        return File.Exists(specPath)
            ? Results.File(specPath, "application/json")
            : Results.NotFound(new { message = "OpenAPI specification file was not found." });
    });
}

await app.RunAsync();

public partial class Program
{
}
