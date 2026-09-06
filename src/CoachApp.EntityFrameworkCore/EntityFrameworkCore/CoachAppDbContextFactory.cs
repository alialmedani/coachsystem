using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CoachApp.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class CoachAppDbContextFactory : IDesignTimeDbContextFactory<CoachAppDbContext>
{
    public CoachAppDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        CoachAppEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<CoachAppDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new CoachAppDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../CoachApp.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
