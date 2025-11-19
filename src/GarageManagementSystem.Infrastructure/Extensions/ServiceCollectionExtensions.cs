using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Repositories;

namespace GarageManagementSystem.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for automatic service registration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Tự động đăng ký tất cả các Repository (I{Name}Repository -> {Name}Repository)
        /// </summary>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            var repositoryAssembly = Assembly.GetAssembly(typeof(UnitOfWork));
            if (repositoryAssembly == null) return services;

            // Lấy tất cả các interface repository từ Core.Interfaces
            var interfaceAssembly = Assembly.GetAssembly(typeof(IUnitOfWork));
            if (interfaceAssembly == null) return services;

            var repositoryInterfaces = interfaceAssembly
                .GetTypes()
                .Where(t => t.IsInterface && 
                           t.Name.EndsWith("Repository") && 
                           t.Name != "IGenericRepository" &&
                           t != typeof(IUnitOfWork))
                .ToList();

            // Lấy tất cả các implementation repository từ Infrastructure.Repositories
            var repositoryImplementations = repositoryAssembly
                .GetTypes()
                .Where(t => t.IsClass && 
                           !t.IsAbstract && 
                           t.Namespace == "GarageManagementSystem.Infrastructure.Repositories" &&
                           t.Name.EndsWith("Repository"))
                .ToList();

            // Đăng ký từng repository
            foreach (var repositoryInterface in repositoryInterfaces)
            {
                // Tìm implementation tương ứng (bỏ "I" prefix)
                var implementationName = repositoryInterface.Name.Substring(1); // Bỏ "I"
                var implementation = repositoryImplementations
                    .FirstOrDefault(impl => impl.Name == implementationName);

                if (implementation != null && repositoryInterface.IsAssignableFrom(implementation))
                {
                    services.AddScoped(repositoryInterface, implementation);
                }
            }

            // Đăng ký UnitOfWork riêng
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        /// <summary>
        /// Tự động đăng ký tất cả các Service (I{Name}Service -> {Name}Service)
        /// Quét từ nhiều namespace: Core.Services, Infrastructure.Services, Shared.Services
        /// </summary>
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Core.Services
            var coreAssembly = Assembly.GetAssembly(typeof(Core.Services.ConfigurationService));
            if (coreAssembly != null)
            {
                RegisterServicesFromAssembly(services, coreAssembly, "Core.Services");
            }

            // Infrastructure.Services
            var infrastructureAssembly = Assembly.GetAssembly(typeof(Infrastructure.Services.ProfitReportService));
            if (infrastructureAssembly != null)
            {
                RegisterServicesFromAssembly(services, infrastructureAssembly, "Infrastructure.Services");
            }

            // Shared.Services
            var sharedAssembly = Assembly.GetAssembly(typeof(Shared.Services.IInvoiceService));
            if (sharedAssembly != null)
            {
                RegisterServicesFromAssembly(services, sharedAssembly, "Shared.Services");
            }

            return services;
        }

        /// <summary>
        /// Đăng ký services từ một assembly cụ thể
        /// </summary>
        private static void RegisterServicesFromAssembly(IServiceCollection services, Assembly assembly, string namespacePrefix)
        {
            // Tìm tất cả các interface service
            var serviceInterfaces = assembly
                .GetTypes()
                .Where(t => t.IsInterface && 
                           t.Name.StartsWith("I") && 
                           t.Name.EndsWith("Service") &&
                           (t.Namespace?.StartsWith(namespacePrefix) == true || 
                            t.Namespace?.Contains("Services") == true))
                .ToList();

            // Tìm tất cả các implementation service
            var serviceImplementations = assembly
                .GetTypes()
                .Where(t => t.IsClass && 
                           !t.IsAbstract && 
                           t.Name.EndsWith("Service") &&
                           !t.Name.StartsWith("I") &&
                           (t.Namespace?.StartsWith(namespacePrefix) == true || 
                            t.Namespace?.Contains("Services") == true))
                .ToList();

            // Đăng ký từng service
            foreach (var serviceInterface in serviceInterfaces)
            {
                // Tìm implementation tương ứng (bỏ "I" prefix)
                var implementationName = serviceInterface.Name.Substring(1); // Bỏ "I"
                var implementation = serviceImplementations
                    .FirstOrDefault(impl => impl.Name == implementationName && 
                                          serviceInterface.IsAssignableFrom(impl));

                if (implementation != null)
                {
                    // Kiểm tra xem có phải Singleton không (ví dụ: CacheService)
                    var isSingleton = implementationName == "CacheService" && 
                                     serviceInterface.Namespace?.Contains("Core.Services") == true;
                    
                    if (isSingleton)
                    {
                        services.AddSingleton(serviceInterface, implementation);
                    }
                    else
                    {
                        services.AddScoped(serviceInterface, implementation);
                    }
                }
            }
        }

        /// <summary>
        /// Tự động đăng ký tất cả Repositories và Services
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddRepositories();
            services.AddServices();
            return services;
        }
    }
}

