using GarageManagementSystem.IdentityServer.Data;
using GarageManagementSystem.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure logging for IdentityServer4
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add detailed logging for authentication
builder.Logging.SetMinimumLevel(LogLevel.Debug);

    // Add services to the container.
    builder.Services.AddDbContext<IdentityDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
            sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

    // Add GaraManagementContext for Claims Management
    builder.Services.AddDbContext<GaraManagementContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

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
    options.UserInteraction.LogoutUrl = "/Account/Logout";
    options.UserInteraction.ErrorUrl = "/Home/Error";

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
    options.ConfigureDbContext = b => b.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => 
        {
            sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
            sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
})
.AddOperationalStore(options =>
{
    options.ConfigureDbContext = b => b.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => 
        {
            sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
            sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
    options.EnableTokenCleanup = true;
    options.TokenCleanupInterval = 30;
})
.AddAspNetIdentity<ApplicationUser>();

// Configure IdentityServer options
builder.Services.Configure<IdentityServer4.Configuration.IdentityServerOptions>(options =>
{
    var identityConfig = builder.Configuration.GetSection("IdentityServer");
    options.IssuerUri = identityConfig["IssuerUri"] ?? "https://localhost:44333";
});

// Register Custom Services
builder.Services.AddScoped<IdentityServer4.Services.IProfileService, GarageManagementSystem.IdentityServer.Services.CustomProfileService>();
builder.Services.AddScoped<GarageManagementSystem.IdentityServer.Services.OptimizedClaimsService>();

// Configure Identity cookie settings after AddIdentityServer
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None; // For HTTP development
    options.Cookie.HttpOnly = true;
});

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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer(); // Thêm IdentityServer4 sau authentication
app.UseAuthorization();

// Seed user data và IdentityServer4 data
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    // Check if admin user exists
    var adminUser = await userManager.FindByEmailAsync("admin@test.com");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User"
        };
        
        var result = await userManager.CreateAsync(adminUser, "123456");
        if (result.Succeeded)
        {
            Console.WriteLine("Admin user created successfully!");
        }
        else
        {
            Console.WriteLine("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
    else
    {
        Console.WriteLine("Admin user already exists!");
    }

        // Seed IdentityServer4 data
        var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    await GarageManagementSystem.IdentityServer.Data.ConfigurationDbContextSeedData.SeedAsync(scope.ServiceProvider);
    Console.WriteLine("IdentityServer4 configuration store ready!");
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
