// Removed direct database dependencies - Web App calls API instead
using GarageManagementSystem.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options =>
{
    // Load configuration from appsettings.json
    var identityConfig = builder.Configuration.GetSection("IdentityServer");
    
    options.Authority = identityConfig["Authority"];
    options.ClientId = identityConfig["ClientId"];
    options.ClientSecret = identityConfig["ClientSecret"];
    options.ResponseType = identityConfig["ResponseType"];
    
    // Add scopes from configuration
    var scopes = identityConfig.GetSection("Scopes").Get<string[]>();
    foreach (var scope in scopes ?? new[] { "openid", "profile" })
    {
        options.Scope.Add(scope);
    }
    
    options.SaveTokens = identityConfig.GetValue<bool>("SaveTokens");
    options.GetClaimsFromUserInfoEndpoint = identityConfig.GetValue<bool>("GetClaimsFromUserInfoEndpoint");
    options.RequireHttpsMetadata = identityConfig.GetValue<bool>("RequireHttpsMetadata");
    
    // Configure logout
    options.SignedOutRedirectUri = identityConfig["SignedOutRedirectUri"] ?? "/Home/Index";
    
    // Configure to handle network issues
    options.BackchannelHttpHandler = new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
    options.BackchannelTimeout = TimeSpan.FromSeconds(identityConfig.GetValue<int>("BackchannelTimeout"));
    
    // Map claims to user properties
    // options.ClaimActions.MapUniqueJsonKey("role", "role");
    // options.ClaimActions.MapUniqueJsonKey("address", "address");
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
