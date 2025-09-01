using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SmartPlanner.Infrastructure.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace SmartPlanner.Tests.Performance;

public abstract class LoadTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;

    protected LoadTestBase(WebApplicationFactory<Program> factory)
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

    protected async Task<string> GetAuthTokenAsync()
    {
        try
        {
            // For session-based authentication, simulate login
            var registerData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", "loadtest@example.com"),
                new KeyValuePair<string, string>("Password", "LoadTest123!"),
                new KeyValuePair<string, string>("Name", "Load Test User")
            });
            
            // Try to register the user (may fail if already exists, which is fine)
            await _client.PostAsync("/Authentication/Register", registerData);
            
            // Then login
            var loginData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", "loadtest@example.com"),
                new KeyValuePair<string, string>("Password", "LoadTest123!")
            });
            
            var response = await _client.PostAsync("/Authentication/Login", loginData);
            
            // Return a dummy token since this uses session-based auth
            return "session-token";
        }
        catch
        {
            // If authentication fails, return empty string
            return string.Empty;
        }
    }
}
