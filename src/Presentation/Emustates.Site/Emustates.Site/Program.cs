using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Emustates.Site.Client.Pages;
using Emustates.Site.Components;
using Emustates.Site.Components.Account;
using Emustates.Site.Data;
using Emustates.Infra.Data;
using Microsoft.Extensions.Hosting;
using Emustates.Site;
using Emustates.Site.IdentityCore;
using System.Threading.RateLimiting;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers(options => { });
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    
    .AddIdentityCookies()
    ;
builder.Services.AddAuthorization();

builder.AddDataEntityFrameworkCore();
builder.AddAspnetIdentityCore();
builder.AddAspnetIdentityImplementation();

// Development behaviors
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Rate limiting: add a policy for test API endpoints partitioned by remote IP
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    // Customize rejection response: return JSON problem details for API requests
    options.OnRejected = async (context, cancellationToken) =>
    {
        try
        {
            var path = context.HttpContext.Request.Path.Value ?? string.Empty;
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.HttpContext.Response.ContentType = "application/problem+json";
                var problem = new
                {
                    type = "https://httpstatuses.com/429",
                    title = "Too Many Requests",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Too many requests. Please try again later."
                };
                var json = JsonSerializer.Serialize(problem);
                await context.HttpContext.Response.WriteAsync(json, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                context.HttpContext.Response.ContentType = "text/plain";
                await context.HttpContext.Response.WriteAsync("Too many requests", cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            // nothing else to do if writing the rejection fails
        }
    };

    options.AddPolicy("test-api", context =>
    {
        // Partition by remote IP so each client gets its own quota
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30, // 30 requests
            Window = TimeSpan.FromMinutes(1), // per minute
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();

    // serve swagger JSON under /api/swagger/{documentName}/swagger.json
    app.UseSwagger(c => c.RouteTemplate = "api/swagger/{documentName}/swagger.json");

    // serve swagger UI under /api/swagger
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "api/swagger";
        c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "v1");
    });
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

// Enable rate limiter middleware so endpoint-level policies are enforced
app.UseRateLimiter();

// Map controllers/endpoints for Swagger discovery and API routing before Blazor fallback
app.MapControllers();

// Return 200 for requests to the API root to avoid Blazor capturing it as a page
app.MapGet("/api", () => Results.Ok("api is healthy"));

// Explicitly handle trailing-slash API root to avoid ambiguous matches
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/api/")
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("Not Found");
        return;
    }
    await next();
});

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Emustates.Site.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
