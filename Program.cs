using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PortfolioSite.Data;
using PortfolioSite.Models.Identity;
using PortfolioSite.Models.Options;
using PortfolioSite.Services;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<AdminAccountOptions>()
    .Bind(builder.Configuration.GetSection("AdminAccount"))
    .PostConfigure(options =>
    {
        options.LoginId = string.IsNullOrWhiteSpace(options.LoginId) ? "admin" : options.LoginId.Trim();
        options.Password = options.Password?.Trim() ?? string.Empty;
    })
    .Validate(options => !string.IsNullOrWhiteSpace(options.LoginId), "AdminAccount:LoginId must not be empty.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Password)
            && !string.Equals(options.Password, "__SET_BY_SECRETS_OR_ENV__", StringComparison.Ordinal),
        "AdminAccount:Password must be supplied via user-secrets or environment variable.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ReverseProxyOptions>()
    .Bind(builder.Configuration.GetSection("ReverseProxy"));

var connectionString = builder.Configuration.GetConnectionString("PortfolioDatabase");
if (string.IsNullOrWhiteSpace(connectionString)
    || string.Equals(connectionString, "__SET_BY_SECRETS_OR_ENV__", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "Connection string 'PortfolioDatabase' is not configured. Set it via dotnet user-secrets for development or environment variable/env file for runtime.");
}

builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["image/svg+xml"]);
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.AddDbContextPool<PortfolioDbContext>(options => PortfolioDatabaseOptions.Configure(options, connectionString));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
});

builder.Services.AddIdentity<AdminUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 12;
        options.Password.RequiredUniqueChars = 4;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<PortfolioDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
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
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddScoped<PortfolioContentService>();
builder.Services.AddScoped<PortfolioDbInitializer>();

var app = builder.Build();

var reverseProxyOptions = app.Services.GetRequiredService<IOptions<ReverseProxyOptions>>().Value;
if (reverseProxyOptions.TrustAllProxies)
{
    var forwardedHeadersOptions = app.Services.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
    forwardedHeadersOptions.KnownNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
}

app.UseForwardedHeaders();
app.UseResponseCompression();

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
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = "public,max-age=2592000,immutable";
        context.Context.Response.Headers.Expires = DateTimeOffset.UtcNow.AddDays(30).ToString("R");
    }
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
