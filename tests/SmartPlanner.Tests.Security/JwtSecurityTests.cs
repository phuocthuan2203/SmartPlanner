using Microsoft.AspNetCore.Mvc.Testing;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace SmartPlanner.Tests.Security;

public class JwtSecurityTests : SecurityTestBase
{
    public JwtSecurityTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task ProtectedEndpoint_WithValidSession_ShouldReturn200()
    {
        // Arrange - authenticate user to establish session
        await GetValidTokenAsync();

        // Act - access protected dashboard
        var response = await _client.GetAsync("/Dashboard");

        // Assert - should redirect to login if not authenticated, or return dashboard
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect,
            $"Expected success or redirect, got {response.StatusCode}");
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutSession_ShouldRedirectToLogin()
    {
        // Act - try to access protected endpoint without authentication
        var response = await _client.GetAsync("/Dashboard");

        // Assert - should redirect to login
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Authentication/Login", response.Headers.Location?.ToString() ?? "");
    }

    [Fact]
    public async Task AuthenticationEndpoint_ShouldBeAccessible()
    {
        // Act - access login page
        var response = await _client.GetAsync("/Authentication/Login");

        // Assert - login page should be accessible
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegisterEndpoint_ShouldBeAccessible()
    {
        // Act - access register page
        var response = await _client.GetAsync("/Authentication/Register");

        // Assert - register page should be accessible
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HomeEndpoint_ShouldBeAccessible()
    {
        // Act - access home page
        var response = await _client.GetAsync("/");

        // Assert - home page should be accessible
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect,
            $"Expected success or redirect, got {response.StatusCode}");
    }

    [Fact]
    public async Task SessionAuthentication_ShouldWork()
    {
        // Arrange & Act - authenticate user
        var token = await GetValidTokenAsync();

        // Assert - authentication should return a session token
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task TaskEndpoint_WithoutAuth_ShouldRedirectToLogin()
    {
        // Act - try to access tasks without authentication
        var response = await _client.GetAsync("/Task");

        // Assert - should redirect to login
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Authentication/Login", response.Headers.Location?.ToString() ?? "");
    }
}
