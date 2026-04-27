using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Work_Dashboard.Components;
using Work_Dashboard.Data;
using Work_Dashboard.Data.Entities;
using Work_Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<BdaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ── Authentication ────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/login";
        options.LogoutPath       = "/account/logout";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name      = "bda.auth";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IAuthService, AuthService>();

// ── Blazor ────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

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
    HttpContext         ctx,
    IAuthService        authService,
    [FromForm] string   username,
    [FromForm] string   password,
    [FromForm] string?  returnUrl) =>
{
    var engineer = await authService.ValidateAsync(username, password);

    if (engineer is null)
        return Results.Redirect("/login?error=1");

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

    var redirect = engineer.Role == EngineerRole.ADMIN ? "/admin" : "/jen";
    return Results.Redirect(redirect);

}).DisableAntiforgery();

app.MapPost("/account/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");

}).DisableAntiforgery();

// ── Blazor ────────────────────────────────────────────────────
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
