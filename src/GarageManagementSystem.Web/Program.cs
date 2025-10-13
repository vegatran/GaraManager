// Removed direct database dependencies - Web App calls API instead
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Web.Middleware;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to handle larger headers
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 262144; // 256KB - tăng gấp đôi
    options.Limits.MaxRequestLineSize = 65536; // 64KB - tăng gấp đôi
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add global authorization policy - require authentication for all endpoints by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Authentication - IdentityServer4 best practices
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc"; // Redirect trực tiếp về IdentityServer
    options.DefaultSignInScheme = "Cookies";
    options.DefaultSignOutScheme = "oidc";
})
.AddCookie("Cookies", options =>
{
    // Cookie configuration based on IdentityServer4 best practices
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cookie expires after 8 hours
    options.SlidingExpiration = true; // Reset expiry on activity
    // options.LoginPath = "/Home/Login"; // ❌ XÓA DÒNG NÀY để redirect trực tiếp đến IdentityServer
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ReturnUrlParameter = "returnUrl";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name = "GarageManagement.Auth";
    options.Cookie.IsEssential = true; // Essential for authentication
    
    // Limit cookie size to prevent "Request Too Long" errors
    options.Cookie.MaxAge = TimeSpan.FromHours(8);
})
.AddOpenIdConnect("oidc", options =>
{
    // Load configuration from appsettings.json
    var identityConfig = builder.Configuration.GetSection("IdentityServer");
    
    options.Authority = identityConfig["Authority"];
    options.ClientId = identityConfig["ClientId"];
    options.ClientSecret = identityConfig["ClientSecret"];
    options.ResponseType = identityConfig["ResponseType"] ?? "code";
    
    // Add scopes from configuration - IdentityServer4 official pattern
    var scopes = identityConfig.GetSection("Scopes").Get<string[]>();
    foreach (var scope in scopes ?? new[] { "openid", "profile" })
    {
        options.Scope.Add(scope);
    }
    
    // IdentityServer4 official configuration - Get from appsettings
    options.SaveTokens = identityConfig.GetValue<bool>("SaveTokens");
    options.GetClaimsFromUserInfoEndpoint = identityConfig.GetValue<bool>("GetClaimsFromUserInfoEndpoint");
    options.RequireHttpsMetadata = identityConfig.GetValue<bool>("RequireHttpsMetadata");
    
    // IdentityServer4 Refresh Token configuration
    options.UsePkce = false; // Disable PKCE for testing
    options.AccessDeniedPath = "/Home/AccessDenied";
    
    // Configure to handle network issues
    options.BackchannelHttpHandler = new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
    
    // Events
    options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
    {
        OnRemoteFailure = context =>
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("Authentication failed: {Error}", context.Exception?.Message);
            return Task.CompletedTask;
        },
        OnUserInformationReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            // logger.LogInformation("User information received for: {Subject}", context.User?.Identity?.Name ?? "Unknown");
            return Task.CompletedTask;
        }
    };
});

// Configure API settings from appsettings.json
var apiConfig = builder.Configuration.GetSection("ApiConfiguration");
ApiConfiguration.BaseUrl = apiConfig["BaseUrl"] ?? "https://localhost:44303/api/";
ApiConfiguration.TimeoutSeconds = int.Parse(apiConfig["TimeoutSeconds"] ?? "30");

// HTTP Client for API calls
builder.Services.AddHttpClient(ApiConfiguration.HttpClientName, client =>
{
    client.BaseAddress = new Uri(ApiConfiguration.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(ApiConfiguration.TimeoutSeconds);
});

// Register ApiService
builder.Services.AddScoped<GarageManagementSystem.Web.Services.ApiService>();
builder.Services.AddHttpContextAccessor();

// Remove direct database connections - Web App will call API instead

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// No direct database operations - Web App calls API instead

app.Run();
