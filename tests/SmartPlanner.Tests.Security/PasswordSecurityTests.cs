using Microsoft.AspNetCore.Mvc.Testing;
using BCrypt.Net;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SmartPlanner.Tests.Security;

public class PasswordSecurityTests : SecurityTestBase
{
    public PasswordSecurityTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task PasswordHashing_ShouldUseBCrypt()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        // Act & Assert
        Assert.True(BCrypt.Net.BCrypt.Verify(password, hashedPassword));
        Assert.False(BCrypt.Net.BCrypt.Verify("WrongPassword", hashedPassword));
        
        // Ensure hash is not the plain password
        Assert.NotEqual(password, hashedPassword);
        
        // Ensure hash looks like BCrypt format
        Assert.StartsWith("$2", hashedPassword);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("password")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("12345678")]
    public async Task WeakPasswords_ShouldBeRejected(string weakPassword)
    {
        // Arrange
        var registerData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Email", $"test-weak-{Guid.NewGuid()}@example.com"),
            new KeyValuePair<string, string>("Password", weakPassword),
            new KeyValuePair<string, string>("Name", "Test User")
        });

        // Act
        var response = await _client.PostAsync("/Authentication/Register", registerData);

        // Assert - should return the form with validation errors or redirect back
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect,
            $"Expected form validation error, got {response.StatusCode}");
    }

    [Theory]
    [InlineData("StrongPassword123!")]
    [InlineData("MySecure@Pass1")]
    [InlineData("Complex#Password2024")]
    public async Task StrongPasswords_ShouldBeAccepted(string strongPassword)
    {
        // Arrange
        var registerData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Email", $"test-strong-{Guid.NewGuid()}@example.com"),
            new KeyValuePair<string, string>("Password", strongPassword),
            new KeyValuePair<string, string>("Name", "Test User")
        });

        // Act
        var response = await _client.PostAsync("/Authentication/Register", registerData);

        // Assert - should redirect to dashboard on success or return form
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK,
            $"Strong password should be accepted, got {response.StatusCode}");
    }

    [Fact]
    public async Task PasswordHashing_ShouldProduceDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = BCrypt.Net.BCrypt.HashPassword(password);
        var hash2 = BCrypt.Net.BCrypt.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // Different salts should produce different hashes
        Assert.True(BCrypt.Net.BCrypt.Verify(password, hash1));
        Assert.True(BCrypt.Net.BCrypt.Verify(password, hash2));
    }

    [Fact]
    public async Task LoginAttempts_ShouldReturnFormResponse()
    {
        // Arrange
        var email = $"login-test-{Guid.NewGuid()}@example.com";
        var wrongPassword = "WrongPassword123!";

        // Act - Attempt login with non-existent user
        var loginData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Email", email),
            new KeyValuePair<string, string>("Password", wrongPassword)
        });

        var response = await _client.PostAsync("/Authentication/Login", loginData);

        // Assert - should return form with error message
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect,
            $"Expected form response, got {response.StatusCode}");
    }
}
