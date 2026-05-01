using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Work_Dashboard.Components;
using Work_Dashboard.Data;
using Work_Dashboard.Data.Entities;
using Work_Dashboard.Services;

// ── Serilog bootstrap logger (captures startup errors) ────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// ── Serilog full configuration ────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/bda-dashboard-.log",
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 10 * 1024 * 1024,   // 10 MB
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: 60,
        shared: false));

// ── Database ──────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<BdaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ── Authentication ────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath         = "/login";
        options.LogoutPath        = "/account/logout";
        options.AccessDeniedPath  = "/login";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name       = "bda.auth";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<Work_Dashboard.Services.FileUploadService>();

// Allow larger SignalR messages so InputFile can stream files up to 25 MB
builder.Services.Configure<Microsoft.AspNetCore.SignalR.HubOptions>(o =>
{
    o.MaximumReceiveMessageSize = 25 * 1024 * 1024; // 25 MB
});

// ── Blazor ────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ── Seed default admin on first run ──────────────────────────
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BdaDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();   // create schema if DB is empty
    if (!await db.Engineers.AnyAsync())
    {
        db.Engineers.Add(new Engineer
        {
            Name         = "Administrator",
            Username     = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
            Role         = EngineerRole.ADMIN,
            IsActive     = true
        });
        await db.SaveChangesAsync();
    }
}

// ── Seed CSV data (once; guard inside SeedAsync) ─────────────
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BdaDbContext>>();
    await Work_Dashboard.Data.DataSeeder.SeedAsync(factory);
}

// ── Pipeline ──────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ── Auth Endpoints ────────────────────────────────────────────
app.MapPost("/account/login", async (
    HttpContext        ctx,
    IAuthService       authService,
    [FromForm] string  username,
    [FromForm] string  password,
    [FromForm] string? returnUrl) =>
{
    var engineer = await authService.ValidateAsync(username, password);
    if (engineer is null) return Results.Redirect("/login?error=1");

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, engineer.EngineerId.ToString()),
        new(ClaimTypes.Name,           engineer.Name),
        new(ClaimTypes.Role,           engineer.Role.ToString()),
        new("username",                engineer.Username),
    };

    var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return engineer.Role switch
    {
        EngineerRole.ADMIN       => Results.Redirect("/admin"),
        EngineerRole.ACCOUNTANT  => Results.Redirect("/accountant"),
        _                        => Results.Redirect("/jen")
    };

}).DisableAntiforgery();

app.MapPost("/account/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).DisableAntiforgery();

// ── Blazor ────────────────────────────────────────────────────
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

try
{
    Log.Information("BDA Work Dashboard starting up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
