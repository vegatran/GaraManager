using GarageManagementSystem.IdentityServer.Data;
using GarageManagementSystem.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to handle larger URLs - Tăng cao hơn nữa
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestLineSize = 131072; // 128KB - tăng gấp 4
    options.Limits.MaxRequestHeadersTotalSize = 524288; // 512KB - tăng gấp 4
});

// Configure logging for IdentityServer4
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add detailed logging for authentication
builder.Logging.SetMinimumLevel(LogLevel.Debug);

    // MySQL Server Version
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

    // Add services to the container.
    builder.Services.AddDbContext<IdentityDbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion));

    // Add GaraManagementContext for Claims Management
    builder.Services.AddDbContext<GaraManagementContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<IdentityDbContext>()
.AddDefaultTokenProviders();

// IdentityServer4 - Cấu hình theo tài liệu chính thức
builder.Services.AddIdentityServer(options =>
{
    // Events configuration
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    
    // User interaction configuration
    options.UserInteraction.LoginUrl = "/Account/Login";
    options.UserInteraction.ErrorUrl = "/Home/Error";
    // Không set LogoutUrl để IdentityServer4 dùng flow chuẩn /connect/endsession
    // options.UserInteraction.ShowLogoutPrompt = false; // Không có property này trong IdentityServer4

    // Endpoints configuration
    options.Endpoints.EnableAuthorizeEndpoint = true;
    options.Endpoints.EnableTokenEndpoint = true;
    options.Endpoints.EnableUserInfoEndpoint = true;
    options.Endpoints.EnableDiscoveryEndpoint = true;
})
.AddDeveloperSigningCredential() // Thêm signing credential cho development
// Temporarily disable Entity Framework stores to avoid AutoMapper conflicts
.AddConfigurationStore<ConfigurationDbContext>(options =>
{
    options.ConfigureDbContext = b => b.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion,
        mySql => mySql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
})
.AddOperationalStore(options =>
{
    options.ConfigureDbContext = b => b.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion,
        mySql => mySql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
    options.EnableTokenCleanup = true;
    options.TokenCleanupInterval = 30;
})
.AddAspNetIdentity<ApplicationUser>();
// .AddProfileService<GarageManagementSystem.IdentityServer.Services.CustomProfileService>(); // Temporarily disabled

// Register Custom Services
builder.Services.AddScoped<GarageManagementSystem.IdentityServer.Services.OptimizedClaimsService>();

// Configure Identity cookie settings after AddIdentityServer
// Comment out to avoid IIS Express conflict
/*
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None; // For HTTP development
    options.Cookie.HttpOnly = true;
});
*/

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

// Add Memory Cache for better performance with optimized settings
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000; // Maximum number of cache entries
    options.CompactionPercentage = 0.25; // Remove 25% of entries when limit is reached
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // Scan for expired entries every 5 minutes
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Disable HTTPS redirection for development
if (app.Environment.IsDevelopment())
{
    // Skip HTTPS redirection in development
}
else
{
    app.UseHttpsRedirection();
}

// Disable BrowserLink and Browser Refresh in development to fix CSP issues
if (!app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
}
else
{
    app.UseStaticFiles();
}

app.UseRouting();

// Disable CSP in development to avoid BrowserLink conflicts
// BrowserLink is disabled in project file to prevent reload issues

app.UseAuthentication();
app.UseIdentityServer(); // Thêm IdentityServer4 sau authentication
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
