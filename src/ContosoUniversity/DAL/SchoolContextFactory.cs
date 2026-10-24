namespace ContosoUniversity.DAL
{
    using System;
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Used by the EF Core command line tools (dotnet ef migrations add / database update) so the
    /// tooling can build a SchoolContext without starting the web host.
    /// </summary>
    public class SchoolContextFactory : IDesignTimeDbContextFactory<SchoolContext>
    {
        public SchoolContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("SchoolContext")
                ?? "Server=(localdb)\\mssqllocaldb;Database=ContosoUniversity;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True";

            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder.UseSqlServer(connectionString, o => o.EnableRetryOnFailure());

            return new SchoolContext(optionsBuilder.Options);
        }
    }
}
