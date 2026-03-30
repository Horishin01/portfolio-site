using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PortfolioSite.Models.Options;

namespace PortfolioSite.Services;

public sealed class AdminCredentialService
{
    private readonly PasswordHashService _passwordHashService;
    private readonly AdminAccountOptions _options;

    public AdminCredentialService(
        PasswordHashService passwordHashService,
        IOptions<AdminAccountOptions> optionsAccessor
    )
    {
        _passwordHashService = passwordHashService;
        _options = optionsAccessor.Value;
    }

    public string LoginId => _options.LoginId;

    public bool Validate(string loginId, string password)
    {
        if (!FixedTimeEquals(loginId.Trim(), _options.LoginId))
        {
            return false;
        }

        return _passwordHashService.VerifyPassword(_options.PasswordHash, password);
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
