using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Services;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.Infrastructure.Extensions;
using GarageManagementSystem.Infrastructure.Repositories;
using GarageManagementSystem.Infrastructure.Services;
using GarageManagementSystem.Shared.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64;
    });

// JWT Bearer Authentication for Duende IdentityServer
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Authority"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("IdentityServer:RequireHttpsMetadata");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = builder.Configuration.GetValue<bool>("IdentityServer:ValidateAudience"),
            ValidAudience = builder.Configuration["IdentityServer:Audience"],
            ValidateIssuer = builder.Configuration.GetValue<bool>("IdentityServer:ValidateIssuer"),
            ValidIssuer = builder.Configuration["IdentityServer:Issuer"],
            ValidateLifetime = builder.Configuration.GetValue<bool>("IdentityServer:ValidateLifetime")
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "garage.api");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Garage Management API", 
        Version = "v1",
        Description = "Comprehensive API for Garage Management System including customer, vehicle, service, inventory, and financial management.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Garage Management System",
            Email = "support@garagemanagement.com"
        }
    });
    
    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // Tag descriptions
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default" });
    c.DocInclusionPredicate((name, api) => true);
});

// Add HttpContextAccessor for AuditInterceptor
builder.Services.AddHttpContextAccessor();

// Add AuditInterceptor
builder.Services.AddScoped<GarageManagementSystem.Infrastructure.Interceptors.AuditInterceptor>();

// MySQL Server Version
var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

// Database with AuditInterceptor and HttpContextAccessor
builder.Services.AddDbContext<GarageDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetRequiredService<GarageManagementSystem.Infrastructure.Interceptors.AuditInterceptor>();
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion)
           .AddInterceptors(auditInterceptor);
});

// ✅ Tự động đăng ký tất cả Repositories và Services
// Sử dụng extension method để tự động quét và đăng ký I{Name}Repository -> {Name}Repository
// và I{Name}Service -> {Name}Service
builder.Services.AddApplicationServices();

// ✅ FIX: Đăng ký các Services nằm trong Core.Interfaces (không được auto-register bởi AddApplicationServices)
// Các service này có interface trong Core.Interfaces nhưng implementation ở Infrastructure hoặc Core.Services
builder.Services.AddScoped<GarageManagementSystem.Core.Interfaces.ICOGSCalculationService, GarageManagementSystem.Infrastructure.Services.COGSCalculationService>();
builder.Services.AddScoped<GarageManagementSystem.Core.Interfaces.IWarrantyService, GarageManagementSystem.Infrastructure.Services.WarrantyService>();
builder.Services.AddScoped<GarageManagementSystem.Core.Interfaces.IProfitReportService, GarageManagementSystem.Infrastructure.Services.ProfitReportService>();
builder.Services.AddScoped<GarageManagementSystem.Core.Interfaces.IFinancialTransactionService, GarageManagementSystem.Infrastructure.Services.FinancialTransactionService>();
builder.Services.AddScoped<GarageManagementSystem.Core.Interfaces.IPrintTemplateService, GarageManagementSystem.Core.Services.PrintTemplateService>();

// Cache Service (Singleton - đăng ký riêng vì cần Singleton)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

// SignalR
builder.Services.AddSignalR();

// API Services (đăng ký riêng vì nằm trong API project)
builder.Services.AddScoped<GarageManagementSystem.API.Services.INotificationService, GarageManagementSystem.API.Services.NotificationService>();
// Background Jobs
builder.Services.AddHostedService<GarageManagementSystem.API.Services.BackgroundJobService>();
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7000",   // Web app HTTPS
                "http://localhost:7003",    // Web app HTTP
                "https://localhost:7001",   // Alternative Web app port
                "http://localhost:7002",    // Alternative Web app port
                "https://localhost:44352",  // ✅ FIX: Web app development port
                "http://localhost:44352",   // ✅ FIX: Web app HTTP (if any)
                "https://localhost:44303",  // ✅ FIX: API development port (for same-origin requests)
                "http://localhost:44303"    // ✅ FIX: API HTTP (if any)
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
    
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
// Add Memory Cache
builder.Services.AddMemoryCache();
// Add Cache Service
builder.Services.AddScoped<GarageManagementSystem.API.Services.ICacheService, GarageManagementSystem.API.Services.CacheService>();

var app = builder.Build();

// Configure custom middleware
app.UseMiddleware<GarageManagementSystem.API.Middleware.ErrorHandlingMiddleware>();
app.UseMiddleware<GarageManagementSystem.API.Middleware.RequestLoggingMiddleware>();
app.UseMiddleware<GarageManagementSystem.API.Middleware.RateLimitingMiddleware>();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ✅ FIX: SignalR Hub - Map after CORS to ensure CORS headers are sent
// Note: SignalR hub access should be controlled via JavaScript authentication if needed
app.MapHub<GarageManagementSystem.API.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
