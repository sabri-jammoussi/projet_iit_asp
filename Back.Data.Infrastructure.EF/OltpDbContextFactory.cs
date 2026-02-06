using System.IO;
using Back.Data.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Back.Data.Infrastructure.EF
{
    // Design-time factory used by EF Tools (migrations)
    public class OltpDbContextFactory : IDesignTimeDbContextFactory<OltpDbContext>
    {
        public OltpDbContext CreateDbContext(string[] args)
        {
            // Find appsettings.json by walking up the directory tree
            var basePath = Directory.GetCurrentDirectory();
            while (basePath != null && !File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                var parent = Directory.GetParent(basePath);
                basePath = parent?.FullName;
            }

            var builder = new ConfigurationBuilder();
            if (basePath != null)
            {
                builder.SetBasePath(basePath).AddJsonFile("appsettings.json", optional: true);
            }
            builder.AddEnvironmentVariables();
            var configuration = builder.Build();

            var connectionString = configuration.GetConnectionString("APP")
                ?? configuration["ConnectionStrings:APP"]
                ?? System.Environment.GetEnvironmentVariable("ConnectionStrings__APP");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'APP' not found. Set it in appsettings.json or environment variables.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<OltpDbContext>();
            optionsBuilder.UseSqlServer(connectionString, o => o.UseCompatibilityLevel(120));

            var loggerFactory = LoggerFactory.Create(_ => { });
            var logger = loggerFactory.CreateLogger<OltpDbContext>();

            return new OltpDbContext(optionsBuilder.Options, configuration, logger);
        }
    }
}
