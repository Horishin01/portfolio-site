using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Data;
using PortfolioSite.Models.Identity;
using PortfolioSite.Models.Options;
using PortfolioSite.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<AdminAccountOptions>()
    .Bind(builder.Configuration.GetSection("AdminAccount"))
    .PostConfigure(options =>
    {
        options.LoginId = string.IsNullOrWhiteSpace(options.LoginId) ? "admin" : options.LoginId.Trim();
        options.Password = string.IsNullOrWhiteSpace(options.Password) ? "0000" : options.Password.Trim();
    })
    .Validate(options => !string.IsNullOrWhiteSpace(options.LoginId), "AdminAccount:LoginId must not be empty.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "AdminAccount:Password must not be empty.")
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("PortfolioDatabase")
    ?? throw new InvalidOperationException("Connection string 'PortfolioDatabase' is not configured.");

builder.Services.AddDbContext<PortfolioDbContext>(options => PortfolioDatabaseOptions.Configure(options, connectionString));

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
builder.Services.AddControllersWithViews();
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
