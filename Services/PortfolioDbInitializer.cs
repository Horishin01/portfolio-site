using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
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
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AdminUser> _userManager;

    public PortfolioDbInitializer(
        PortfolioContentService contentService,
        PortfolioDbContext dbContext,
        UserManager<AdminUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<AdminAccountOptions> adminAccountOptionsAccessor
    )
    {
        _adminAccountOptions = adminAccountOptionsAccessor.Value;
        _contentService = contentService;
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.IsSqlite())
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
        }

        await EnsureAdminUserAsync();

        if (!await _contentService.HasContentAsync(cancellationToken))
        {
            await _contentService.ResetToSeedAsync(cancellationToken);
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
                LockoutEnabled = true,
                PasswordHash = _adminAccountOptions.PasswordHash
            };

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

            if (!string.Equals(user.PasswordHash, _adminAccountOptions.PasswordHash, StringComparison.Ordinal))
            {
                user.PasswordHash = _adminAccountOptions.PasswordHash;
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
