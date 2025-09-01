using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartPlanner.Application.Services;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Infrastructure.Data;
using SmartPlanner.Infrastructure.Security;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework
builder.Services.AddDbContext<SmartPlannerDbContext>(options =>
    {
        var conn = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseNpgsql(conn);
    });

// Persist DataProtection keys (for stable auth/antiforgery/session cookies across restarts)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/lib/smartplanner-keys"))
    .SetApplicationName("SmartPlanner");

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register repositories
builder.Services.AddScoped<SmartPlanner.Infrastructure.Repositories.ITaskRepository, SmartPlanner.Infrastructure.Repositories.TaskRepository>();
builder.Services.AddScoped<SmartPlanner.Infrastructure.Repositories.ISubjectRepository, SmartPlanner.Infrastructure.Repositories.SubjectRepository>();

// Register services
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITaskService, SmartPlanner.Application.Services.TaskService>();
builder.Services.AddScoped<ISubjectService, SmartPlanner.Application.Services.SubjectService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();


// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ensure Secure over HTTPS
});
var app = builder.Build();

//Trust proxy headers from Nginx and set original scheme/IP before HSTS/HTTPS
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

// Auto-apply EF Core migrations in Production on startup
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SmartPlannerDbContext>();
    try { db.Database.Migrate(); }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration failed on startup");
        throw; // fail fast so systemd restarts the service
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Make the Program class accessible for integration tests
public partial class Program { }
