using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GarageManagementSystem.Infrastructure.Data
{
    public class GarageDbContextFactory : IDesignTimeDbContextFactory<GarageDbContext>
    {
        public GarageDbContext CreateDbContext(string[] args)
        {
            // Đọc từ appsettings.json của API project
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "GarageManagementSystem.API"))
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

            var optionsBuilder = new DbContextOptionsBuilder<GarageDbContext>();
            optionsBuilder.UseMySql(connectionString, serverVersion);

            return new GarageDbContext(optionsBuilder.Options);
        }
    }
}

