using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Data;
using PortfolioSite.Models.Options;
using PortfolioSite.Services;

if (args.Length > 0 && string.Equals(args[0], "hash-password", StringComparison.OrdinalIgnoreCase))
{
    var password = args.Length > 1 ? args[1] : PromptPassword();

    if (string.IsNullOrWhiteSpace(password))
    {
        throw new InvalidOperationException("Password is required.");
    }

    var passwordHasher = new PasswordHasher<object>();
    Console.WriteLine(passwordHasher.HashPassword(new object(), password));
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<AdminAccountOptions>()
    .Bind(builder.Configuration.GetSection(AdminAccountOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.LoginId), "AdminAccount:LoginId is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.PasswordHash), "AdminAccount:PasswordHash is required.")
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("PortfolioDatabase")
    ?? throw new InvalidOperationException("Connection string 'PortfolioDatabase' is not configured.");

builder.Services.AddDbContext<PortfolioDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 36)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    );
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "portfolio_admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PasswordHashService>();
builder.Services.AddScoped<AdminCredentialService>();
builder.Services.AddScoped<PortfolioContentService>();
builder.Services.AddScoped<PortfolioDbInitializer>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<PortfolioDbInitializer>();
    await initializer.InitializeAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();

static string PromptPassword()
{
    Console.Write("Password: ");
    return Console.ReadLine()?.Trim() ?? "";
}
