using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.Infrastructure.Repositories;
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

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IVehicleInspectionRepository, VehicleInspectionRepository>();
builder.Services.AddScoped<IServiceQuotationRepository, ServiceQuotationRepository>();
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IPrintTemplateRepository, PrintTemplateRepository>();
builder.Services.AddScoped<ICustomerReceptionRepository, CustomerReceptionRepository>();

// Services
builder.Services.AddScoped<GarageManagementSystem.Core.Interfaces.IPrintTemplateService, GarageManagementSystem.Core.Services.PrintTemplateService>();

// Excel Import Service
builder.Services.AddScoped<GarageManagementSystem.Core.Services.IExcelImportService, GarageManagementSystem.Infrastructure.Services.ExcelImportService>();

// Invoice Service
builder.Services.AddScoped<GarageManagementSystem.Shared.Services.IInvoiceService, GarageManagementSystem.Infrastructure.Services.InvoiceService>();

// ✅ 3.1: COGS Calculation Service
builder.Services.AddScoped<GarageManagementSystem.Core.Interfaces.ICOGSCalculationService, GarageManagementSystem.Infrastructure.Services.COGSCalculationService>();

// Configuration Service
builder.Services.AddScoped<GarageManagementSystem.Core.Services.IConfigurationService, GarageManagementSystem.Core.Services.ConfigurationService>();

// Cache Service
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<GarageManagementSystem.Core.Services.ICacheService, GarageManagementSystem.Core.Services.CacheService>();
// Audit Log Service
builder.Services.AddScoped<GarageManagementSystem.Core.Services.IAuditLogService, GarageManagementSystem.Core.Services.AuditLogService>();

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
                "http://localhost:7002"     // Alternative Web app port
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

// Ensure database is created
//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<GarageDbContext>();
//    context.Database.EnsureCreated();
//}

app.Run();
