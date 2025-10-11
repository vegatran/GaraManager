using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarageManagementSystem.IdentityServer.Data
{
    public class PersistedGrantDbContextFactory : IDesignTimeDbContextFactory<PersistedGrantDbContext>
    {
        public PersistedGrantDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

            var services = new ServiceCollection();
            services.AddEntityFrameworkMySql();
            services.AddSingleton(new OperationalStoreOptions());
            var serviceProvider = services.BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder<PersistedGrantDbContext>();
            optionsBuilder.UseMySql(connectionString, serverVersion,
                mySqlOptions => mySqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
            optionsBuilder.UseInternalServiceProvider(serviceProvider);

            return new PersistedGrantDbContext(optionsBuilder.Options);
        }
    }
}

