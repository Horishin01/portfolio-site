using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace PortfolioSite.Data;

public static class PortfolioDatabaseOptions
{
    public static void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        if (UsesSqlite(connectionString))
        {
            EnsureSqliteDirectoryExists(connectionString);
            optionsBuilder.UseSqlite(connectionString);
            return;
        }

        if (UsesPostgres(connectionString))
        {
            optionsBuilder.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
            );
            return;
        }

        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 36)),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure()
        );
    }

    public static bool UsesSqlite(string connectionString)
    {
        var normalized = connectionString.TrimStart();
        return normalized.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("DataSource=", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("Filename=", StringComparison.OrdinalIgnoreCase);
    }

    public static bool UsesPostgres(string connectionString)
    {
        var normalized = connectionString.TrimStart();
        return normalized.StartsWith("Host=", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("Username=", StringComparison.OrdinalIgnoreCase)
            || (normalized.StartsWith("User ID=", StringComparison.OrdinalIgnoreCase)
                && normalized.Contains("Host=", StringComparison.OrdinalIgnoreCase));
    }

    private static void EnsureSqliteDirectoryExists(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.DataSource) || builder.DataSource == ":memory:")
        {
            return;
        }

        var fullPath = Path.GetFullPath(builder.DataSource, Directory.GetCurrentDirectory());
        var directoryPath = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}
