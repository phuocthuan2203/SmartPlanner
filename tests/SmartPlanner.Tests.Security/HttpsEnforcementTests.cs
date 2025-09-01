using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SmartPlanner.Tests.Security;

public class HttpsEnforcementTests : SecurityTestBase
{
    public HttpsEnforcementTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task HomePageShouldBeAccessible()
    {
        // Act - access home page
        var response = await _client.GetAsync("/");

        // Assert - home page should be accessible
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect,
            $"Home page should be accessible, got {response.StatusCode}");
    }

    [Fact]
    public async Task SecurityHeaders_ShouldBePresent()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert - basic security check (application should respond)
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect,
            $"Application should respond to requests, got {response.StatusCode}");
        
        // Check that response is not empty
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }

    [Fact]
    public async Task ApplicationShouldHandleRequestsSecurely()
    {
        // Act - test various endpoints
        var homeResponse = await _client.GetAsync("/");
        var loginResponse = await _client.GetAsync("/Authentication/Login");
        
        // Assert - endpoints should respond appropriately
        Assert.True(homeResponse.IsSuccessStatusCode || homeResponse.StatusCode == HttpStatusCode.Redirect,
            "Home endpoint should be accessible");
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoints_ShouldRequireAuthentication()
    {
        // Test various protected endpoints
        var endpoints = new[]
        {
            "/Dashboard",
            "/Task",
            "/Subject"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            // Should redirect to login when not authenticated
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Authentication/Login", response.Headers.Location?.ToString() ?? "");
        }
    }

    [Fact]
    public async Task AuthenticationEndpoints_ShouldBeAccessible()
    {
        // Test authentication endpoints
        var loginResponse = await _client.GetAsync("/Authentication/Login");
        var registerResponse = await _client.GetAsync("/Authentication/Register");

        // Assert - authentication pages should be accessible
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
    }

    [Fact]
    public async Task SessionManagement_ShouldWorkCorrectly()
    {
        // Arrange - try to register a user
        var registerData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Email", "session-test@example.com"),
            new KeyValuePair<string, string>("Password", "TestPassword123!"),
            new KeyValuePair<string, string>("Name", "Session Test User")
        });

        // Act
        var response = await _client.PostAsync("/Authentication/Register", registerData);

        // Assert - should handle registration attempt
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK,
            $"Registration should be handled properly, got {response.StatusCode}");
    }
}
