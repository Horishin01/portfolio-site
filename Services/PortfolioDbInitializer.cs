using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Data;
using PortfolioSite.Data;
using PortfolioSite.Models.Identity;
using PortfolioSite.Models.Options;

namespace PortfolioSite.Services;

public sealed class PortfolioDbInitializer
{
    private const string BootstrapAdminUserId = "admin";

    private readonly AdminAccountOptions _adminAccountOptions;
    private readonly PortfolioContentService _contentService;
    private readonly PortfolioDbContext _dbContext;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AdminUser> _userManager;

    public PortfolioDbInitializer(
        PortfolioContentService contentService,
        PortfolioDbContext dbContext,
        IPasswordHasher<AdminUser> passwordHasher,
        UserManager<AdminUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<AdminAccountOptions> adminAccountOptionsAccessor
    )
    {
        _adminAccountOptions = adminAccountOptionsAccessor.Value;
        _contentService = contentService;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.IsSqlite())
        {
            if (await HasLegacyJsonSchemaAsync(cancellationToken))
            {
                await _dbContext.Database.EnsureDeletedAsync(cancellationToken);
            }

            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
        }

        await EnsureAdminUserAsync();

        if (!await _contentService.HasContentAsync(cancellationToken))
        {
            await _contentService.ResetToDefaultAsync(cancellationToken);
        }
    }

    private async Task<bool> HasLegacyJsonSchemaAsync(CancellationToken cancellationToken)
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
            command.CommandText = "SELECT 1 FROM pragma_table_info('portfolio_contents') WHERE name = 'JsonContent' LIMIT 1;";
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

            if (!string.Equals(user.UserName, _adminAccountOptions.LoginId, StringComparison.Ordinal))
            {
                user.UserName = _adminAccountOptions.LoginId;
                shouldUpdate = true;
            }

            var passwordVerification = string.IsNullOrWhiteSpace(user.PasswordHash)
                ? PasswordVerificationResult.Failed
                : _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, _adminAccountOptions.Password);

            if (passwordVerification != PasswordVerificationResult.Success)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, _adminAccountOptions.Password);
                user.SecurityStamp = Guid.NewGuid().ToString();
                shouldUpdate = true;
            }

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
