using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Reflection;
using PortfolioSite.Data;
using PortfolioSite.Models.Identity;
using PortfolioSite.Models.Options;

namespace PortfolioSite.Services;

public sealed class PortfolioDbInitializer
{
    private const string BootstrapAdminUserId = "admin";
    private const string InitialCreateMigrationId = "20260330154209_InitialCreate";
    private const string AddIdentityAdminMigrationId = "20260331035809_AddIdentityAdmin";
    private const string NormalizePortfolioContentMigrationId = "20260331043808_NormalizePortfolioContent";
    private const string AddFaviconSettingsMigrationId = "20260331131500_AddFaviconSettings";
    private const string AddPersonalItemImagesMigrationId = "20260408145401_AddPersonalItemImages";
    private const string AddPersonalItemDetailContentMigrationId = "20260408151030_AddPersonalItemDetailContent";

    private readonly AdminAccountOptions _adminAccountOptions;
    private readonly PortfolioContentService _contentService;
    private readonly PortfolioDbContext _dbContext;
    private readonly ILogger<PortfolioDbInitializer> _logger;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AdminUser> _userManager;

    public PortfolioDbInitializer(
        PortfolioContentService contentService,
        PortfolioDbContext dbContext,
        ILogger<PortfolioDbInitializer> logger,
        IPasswordHasher<AdminUser> passwordHasher,
        UserManager<AdminUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<AdminAccountOptions> adminAccountOptionsAccessor
    )
    {
        _adminAccountOptions = adminAccountOptionsAccessor.Value;
        _contentService = contentService;
        _dbContext = dbContext;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseSchemaAsync(cancellationToken);

        await EnsureAdminUserAsync();

        if (!await _contentService.HasContentAsync(cancellationToken))
        {
            await _contentService.ResetToDefaultAsync(cancellationToken);
        }
    }

    private async Task EnsureDatabaseSchemaAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.IsSqlite())
        {
            if (await HasLegacyJsonSchemaAsync(cancellationToken))
            {
                _logger.LogWarning(
                    "Legacy SQLite schema with JsonContent detected. Recreating database so it can be managed by EF migrations.");
                await _dbContext.Database.EnsureDeletedAsync(cancellationToken);
            }

            await BaselineSqliteSchemaIfNeededAsync(cancellationToken);
        }

        await _dbContext.Database.MigrateAsync(cancellationToken);
    }

    private async Task BaselineSqliteSchemaIfNeededAsync(CancellationToken cancellationToken)
    {
        var appliedMigrations = await GetAppliedSqliteMigrationIdsAsync(cancellationToken);
        if (appliedMigrations.Count > 0)
        {
            return;
        }

        if (!await HasAnySqliteUserTablesAsync(cancellationToken))
        {
            return;
        }

        var baselineMigrationId = await DetectSqliteBaselineMigrationIdAsync(cancellationToken);
        if (baselineMigrationId is null)
        {
            throw new InvalidOperationException(
                "The SQLite database has tables but no EF migration history, and its schema could not be safely matched to a known migration baseline.");
        }

        await CreateSqliteMigrationHistoryTableAsync(cancellationToken);

        var migrationIds = _dbContext.Database.GetMigrations().ToList();
        var baselineIndex = migrationIds.IndexOf(baselineMigrationId);
        if (baselineIndex < 0)
        {
            throw new InvalidOperationException($"Migration '{baselineMigrationId}' could not be found in the current assembly.");
        }

        var productVersion = GetEfProductVersion();
        for (var index = 0; index <= baselineIndex; index++)
        {
            var migrationId = migrationIds[index];
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ({migrationId}, {productVersion});""",
                cancellationToken
            );
        }

        _logger.LogInformation(
            "Baselined existing SQLite schema without migration history at migration {MigrationId}.",
            baselineMigrationId
        );
    }

    private async Task<string?> DetectSqliteBaselineMigrationIdAsync(CancellationToken cancellationToken)
    {
        var hasPortfolioContents = await HasSqliteTableAsync("portfolio_contents", cancellationToken);
        if (!hasPortfolioContents)
        {
            return null;
        }

        var hasJsonContent = await HasSqliteColumnAsync("portfolio_contents", "JsonContent", cancellationToken);
        if (hasJsonContent)
        {
            return null;
        }

        var hasIdentityTables = await HasSqliteTableAsync("AspNetUsers", cancellationToken)
            && await HasSqliteTableAsync("AspNetRoles", cancellationToken);
        if (!hasIdentityTables)
        {
            return null;
        }

        var hasNormalizedContent = await HasSqliteColumnAsync("portfolio_contents", "Locale", cancellationToken)
            && await HasSqliteColumnAsync("portfolio_contents", "ContactEmail", cancellationToken)
            && await HasSqliteTableAsync("portfolio_career_items", cancellationToken)
            && await HasSqliteTableAsync("portfolio_contact_links", cancellationToken)
            && await HasSqliteTableAsync("portfolio_skill_categories", cancellationToken)
            && await HasSqliteTableAsync("portfolio_skill_items", cancellationToken)
            && await HasSqliteTableAsync("portfolio_work_items", cancellationToken)
            && await HasSqliteTableAsync("portfolio_personal_items", cancellationToken);

        if (!hasNormalizedContent)
        {
            return AddIdentityAdminMigrationId;
        }

        var hasFavicon = await HasSqliteColumnAsync("portfolio_contents", "FaviconSrc", cancellationToken);
        var hasPersonalImages = await HasSqliteColumnAsync("portfolio_personal_items", "ImageSrc", cancellationToken)
            && await HasSqliteColumnAsync("portfolio_personal_items", "ImageAlt", cancellationToken);
        var hasPersonalDetail = await HasSqliteColumnAsync("portfolio_personal_items", "DetailSummary", cancellationToken)
            && await HasSqliteColumnAsync("portfolio_personal_items", "DetailBody", cancellationToken);

        if (hasPersonalDetail)
        {
            return AddPersonalItemDetailContentMigrationId;
        }

        if (hasPersonalImages)
        {
            return AddPersonalItemImagesMigrationId;
        }

        if (hasFavicon)
        {
            return AddFaviconSettingsMigrationId;
        }

        return NormalizePortfolioContentMigrationId;
    }

    private async Task<bool> HasLegacyJsonSchemaAsync(CancellationToken cancellationToken)
    {
        return await HasSqliteColumnAsync("portfolio_contents", "JsonContent", cancellationToken);
    }

    private async Task CreateSqliteMigrationHistoryTableAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            );
            """,
            cancellationToken
        );
    }

    private async Task<List<string>> GetAppliedSqliteMigrationIdsAsync(CancellationToken cancellationToken)
    {
        if (!await HasSqliteTableAsync("__EFMigrationsHistory", cancellationToken))
        {
            return [];
        }

        var connection = _dbContext.Database.GetDbConnection();
        var openedHere = false;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
            openedHere = true;
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";""";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var migrationIds = new List<string>();
            while (await reader.ReadAsync(cancellationToken))
            {
                migrationIds.Add(reader.GetString(0));
            }

            return migrationIds;
        }
        finally
        {
            if (openedHere)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static string GetEfProductVersion()
    {
        return typeof(DbContext).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion?
            .Split('+', 2)[0]
            ?? "8.0.13";
    }

    private async Task<bool> HasAnySqliteUserTablesAsync(CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var openedHere = false;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
            openedHere = true;
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT 1
                FROM sqlite_master
                WHERE type = 'table'
                  AND name NOT LIKE 'sqlite_%'
                  AND name <> '__EFMigrationsHistory'
                LIMIT 1;
                """;
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is not null && result != DBNull.Value;
        }
        finally
        {
            if (openedHere)
            {
                await connection.CloseAsync();
            }
        }
    }

    private async Task<bool> HasSqliteTableAsync(string tableName, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var openedHere = false;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
            openedHere = true;
        }

        try
        {
            await using var command = connection.CreateCommand();
            var escapedTableName = tableName.Replace("'", "''", StringComparison.Ordinal);
            command.CommandText = $"SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = '{escapedTableName}' LIMIT 1;";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is not null && result != DBNull.Value;
        }
        finally
        {
            if (openedHere)
            {
                await connection.CloseAsync();
            }
        }
    }

    private async Task<bool> HasSqliteColumnAsync(string tableName, string columnName, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var openedHere = false;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
            openedHere = true;
        }

        try
        {
            await using var command = connection.CreateCommand();
            var escapedTableName = tableName.Replace("'", "''", StringComparison.Ordinal);
            var escapedColumnName = columnName.Replace("'", "''", StringComparison.Ordinal);
            command.CommandText = $"SELECT 1 FROM pragma_table_info('{escapedTableName}') WHERE name = '{escapedColumnName}' LIMIT 1;";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is not null && result != DBNull.Value;
        }
        finally
        {
            if (openedHere)
            {
                await connection.CloseAsync();
            }
        }
    }

    private async Task EnsureAdminUserAsync()
    {
        if (!await _roleManager.RoleExistsAsync(AdminRoles.Administrator))
        {
            var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(AdminRoles.Administrator));
            EnsureSucceeded(createRoleResult, "admin role");
        }

        var user = await _userManager.FindByIdAsync(BootstrapAdminUserId)
            ?? await _userManager.FindByNameAsync(_adminAccountOptions.LoginId);

        if (user is null)
        {
            user = new AdminUser
            {
                Id = BootstrapAdminUserId,
                UserName = _adminAccountOptions.LoginId,
                LockoutEnabled = true
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, _adminAccountOptions.Password);

            var createUserResult = await _userManager.CreateAsync(user);
            EnsureSucceeded(createUserResult, "admin user");
        }
        else
        {
            var shouldUpdate = false;

            if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
                shouldUpdate = true;
            }

            if (shouldUpdate)
            {
                var updateUserResult = await _userManager.UpdateAsync(user);
                EnsureSucceeded(updateUserResult, "admin user");
            }
        }

        if (!await _userManager.IsInRoleAsync(user, AdminRoles.Administrator))
        {
            var addToRoleResult = await _userManager.AddToRoleAsync(user, AdminRoles.Administrator);
            EnsureSucceeded(addToRoleResult, "admin role assignment");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string target)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errorMessage = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Failed to initialize {target}: {errorMessage}");
    }
}
