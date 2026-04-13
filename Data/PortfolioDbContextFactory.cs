using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PortfolioSite.Data;

public sealed class PortfolioDbContextFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    private const string DefaultMigrationProvider = "mysql";
    private const string PlaceholderConnectionString = "__SET_BY_SECRETS_OR_ENV__";
    private const string DesignTimeMySqlConnectionString =
        "Server=127.0.0.1;Port=3306;Database=portfolio_site_design_time;User ID=design_time;Password=design_time;CharSet=utf8mb4;";
    private const string DesignTimeSqliteConnectionString =
        "Data Source=LocalData/DesignTime/portfolio-site.design-time.db";

    public PortfolioDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var migrationProvider = ResolveMigrationProvider(args);
        var configuredConnectionString = configuration.GetConnectionString("PortfolioDatabase");
        var connectionString = ResolveConnectionString(migrationProvider, configuredConnectionString);

        var optionsBuilder = new DbContextOptionsBuilder<PortfolioDbContext>();
        PortfolioDatabaseOptions.Configure(optionsBuilder, connectionString);

        return new PortfolioDbContext(optionsBuilder.Options);
    }

    private static string ResolveMigrationProvider(string[] args)
    {
        var provider = ReadArgumentValue(args, "--provider")
            ?? Environment.GetEnvironmentVariable("PORTFOLIO_EF_PROVIDER")
            ?? DefaultMigrationProvider;

        return provider.Trim().ToLowerInvariant() switch
        {
            "mysql" => "mysql",
            "sqlite" => "sqlite",
            _ => throw new InvalidOperationException(
                "Unknown design-time database provider. Use --provider mysql or --provider sqlite.")
        };
    }

    private static string ResolveConnectionString(string migrationProvider, string? configuredConnectionString)
    {
        var hasConfiguredConnectionString =
            !string.IsNullOrWhiteSpace(configuredConnectionString)
            && !string.Equals(configuredConnectionString, PlaceholderConnectionString, StringComparison.Ordinal);

        return migrationProvider switch
        {
            "mysql" when hasConfiguredConnectionString && !PortfolioDatabaseOptions.UsesSqlite(configuredConnectionString!) =>
                configuredConnectionString!,
            "sqlite" when hasConfiguredConnectionString && PortfolioDatabaseOptions.UsesSqlite(configuredConnectionString!) =>
                configuredConnectionString!,
            "mysql" => DesignTimeMySqlConnectionString,
            "sqlite" => DesignTimeSqliteConnectionString,
            _ => throw new InvalidOperationException($"Unsupported migration provider '{migrationProvider}'.")
        };
    }

    private static string? ReadArgumentValue(string[] args, string optionName)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }
}
