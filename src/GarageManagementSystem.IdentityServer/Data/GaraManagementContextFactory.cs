using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GarageManagementSystem.IdentityServer.Data
{
    public class GaraManagementContextFactory : IDesignTimeDbContextFactory<GaraManagementContext>
    {
        public GaraManagementContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

            var optionsBuilder = new DbContextOptionsBuilder<GaraManagementContext>();
            optionsBuilder.UseMySql(connectionString, serverVersion);

            return new GaraManagementContext(optionsBuilder.Options);
        }
    }
}

