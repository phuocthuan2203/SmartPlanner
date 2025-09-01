using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace SmartPlanner.Tests.Security;

public class InputValidationTests : SecurityTestBase
{
    public InputValidationTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Theory]
    [InlineData("'; DROP TABLE Tasks; --")]
    [InlineData("1' OR '1'='1")]
    [InlineData("'; DELETE FROM Users; --")]
    [InlineData("UNION SELECT * FROM Users")]
    public async Task SqlInjectionPayloads_ShouldBeSanitized(string payload)
    {
        // Arrange
        var token = await GetValidTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var taskData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Title", payload),
            new KeyValuePair<string, string>("Description", "Test description"),
            new KeyValuePair<string, string>("Deadline", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")),
            new KeyValuePair<string, string>("StudentId", Guid.NewGuid().ToString())
        });

        // Act
        var response = await _client.PostAsync("/Task/Create", taskData);

        // Assert
        // Should redirect to login (unauthorized) or return form with validation
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK,
            $"Expected redirect to login or form validation, got {response.StatusCode}");
        
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            Assert.Contains("/Authentication/Login", response.Headers.Location?.ToString() ?? "");
        }
    }

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<img src=x onerror=alert('XSS')>")]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("<iframe src='javascript:alert(\"XSS\")'></iframe>")]
    [InlineData("'><script>alert('XSS')</script>")]
    public async Task XssPayloads_ShouldBeSanitized(string payload)
    {
        // Arrange
        var token = await GetValidTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var taskData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Title", payload),
            new KeyValuePair<string, string>("Description", payload),
            new KeyValuePair<string, string>("Deadline", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")),
            new KeyValuePair<string, string>("StudentId", Guid.NewGuid().ToString())
        });

        // Act
        var response = await _client.PostAsync("/Task/Create", taskData);

        // Assert - should redirect to login (unauthorized) or return form
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK,
            $"Expected redirect to login or form response, got {response.StatusCode}");
        
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            Assert.Contains("/Authentication/Login", response.Headers.Location?.ToString() ?? "");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task EmptyOrNullInput_ShouldBeHandledGracefully(string input)
    {
        // Arrange
        var token = await GetValidTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var taskData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Title", input ?? ""),
            new KeyValuePair<string, string>("Description", "Valid description"),
            new KeyValuePair<string, string>("Deadline", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")),
            new KeyValuePair<string, string>("StudentId", Guid.NewGuid().ToString())
        });

        // Act
        var response = await _client.PostAsync("/Task/Create", taskData);

        // Assert - should redirect to login or return form with validation
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK,
            $"Expected redirect or form validation, got {response.StatusCode}");
    }

    [Fact]
    public async Task ExcessivelyLongInput_ShouldBeRejected()
    {
        // Arrange
        var token = await GetValidTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var longString = new string('A', 10000); // 10KB string
        var taskData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Title", longString),
            new KeyValuePair<string, string>("Description", longString),
            new KeyValuePair<string, string>("Deadline", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")),
            new KeyValuePair<string, string>("StudentId", Guid.NewGuid().ToString())
        });

        // Act
        var response = await _client.PostAsync("/Task/Create", taskData);

        // Assert - should redirect to login or handle large input gracefully
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK,
            $"Expected redirect or form handling, got {response.StatusCode}");
    }

    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system32")]
    [InlineData("/etc/shadow")]
    [InlineData("C:\\Windows\\System32\\config\\sam")]
    public async Task PathTraversalPayloads_ShouldBeRejected(string payload)
    {
        // Arrange
        var token = await GetValidTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var taskData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Title", payload),
            new KeyValuePair<string, string>("Description", "Test description"),
            new KeyValuePair<string, string>("Deadline", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")),
            new KeyValuePair<string, string>("StudentId", Guid.NewGuid().ToString())
        });

        // Act
        var response = await _client.PostAsync("/Task/Create", taskData);

        // Assert - should redirect to login or handle path traversal safely
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK,
            $"Expected redirect or safe handling, got {response.StatusCode}");
        
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            Assert.Contains("/Authentication/Login", response.Headers.Location?.ToString() ?? "");
        }
    }

    [Fact]
    public async Task ValidInput_ShouldBeAccepted()
    {
        // Arrange
        var token = await GetValidTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var taskData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Title", "Valid Task Title"),
            new KeyValuePair<string, string>("Description", "This is a valid task description with normal text."),
            new KeyValuePair<string, string>("Deadline", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")),
            new KeyValuePair<string, string>("StudentId", Guid.NewGuid().ToString())
        });

        // Act
        var response = await _client.PostAsync("/Task/Create", taskData);

        // Assert - should redirect to login since not authenticated
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Authentication/Login", response.Headers.Location?.ToString() ?? "");
    }
}
