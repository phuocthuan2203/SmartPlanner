using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SmartPlanner.Infrastructure.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace SmartPlanner.Tests.Security;

public abstract class SecurityTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;

    protected SecurityTestBase(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SmartPlannerDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<SmartPlannerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });
            });
        });
        
        _client = _factory.CreateClient();
        
        // Initialize database
        InitializeDatabase();
    }
    
    private void InitializeDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SmartPlannerDbContext>();
        context.Database.EnsureCreated();
    }

    protected async Task<string> GetValidTokenAsync()
    {
        try
        {
            // For session-based auth, we simulate login by posting to the login endpoint
            var registerData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", "test@example.com"),
                new KeyValuePair<string, string>("Password", "TestPassword123!"),
                new KeyValuePair<string, string>("Name", "Test User")
            });
            
            // Try to register the user (may fail if already exists, which is fine)
            await _client.PostAsync("/Authentication/Register", registerData);
            
            // Then login
            var loginData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", "test@example.com"),
                new KeyValuePair<string, string>("Password", "TestPassword123!")
            });
            
            var response = await _client.PostAsync("/Authentication/Login", loginData);
            
            // For session auth, we don't return a token but the session is established
            return "session-established";
        }
        catch
        {
            // If authentication fails, return empty string
            return string.Empty;
        }
    }

    protected string TamperWithToken(string validToken)
    {
        return "tampered-session-token";
    }

    protected string CreateExpiredToken()
    {
        // This would need to be implemented based on your JWT creation logic
        // For now, return a token that looks valid but is expired
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.invalid";
    }
}
