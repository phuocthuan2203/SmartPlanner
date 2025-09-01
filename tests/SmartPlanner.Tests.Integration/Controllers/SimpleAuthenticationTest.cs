using System.Net;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace SmartPlanner.Tests.Integration.Controllers
{
    public class SimpleAuthenticationTest : IntegrationTestBase
    {
        public SimpleAuthenticationTest(IntegrationTestFixture factory) : base(factory) { }

        [Fact]
        public async Task CanAccessRegisterPage()
        {
            // Act
            var response = await Client.GetAsync("/Authentication/Register");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Register");
        }

        [Fact]
        public async Task RegisterUser_Debug()
        {
            // Arrange
            var registerPageResponse = await Client.GetAsync("/Authentication/Register");
            registerPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var registerDocument = await GetDocumentAsync(registerPageResponse);
            var token = GetAntiForgeryToken(registerDocument);
            token.Should().NotBeNullOrEmpty("Anti-forgery token should be present");

            var registerData = new Dictionary<string, string>
            {
                ["Email"] = "debug.test@example.com",
                ["FullName"] = "Debug Test User",
                ["Password"] = "DebugPassword123!",
                ["ConfirmPassword"] = "DebugPassword123!",
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync("/Authentication/Register", CreateFormContent(registerData));

            // Debug output
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Assert - Registration was successful (HttpClient follows redirects automatically)
            response.StatusCode.Should().Be(HttpStatusCode.OK, "Registration should succeed and redirect to dashboard");
            
            // Verify we're on the dashboard page by checking for dashboard-specific content
            responseContent.Should().Contain("Your Dashboard", "Should be redirected to dashboard page");
            responseContent.Should().Contain("Debug Test User", "User name should appear in the dashboard");
            
            // Check database - user should be created
            using var dbContext = GetDbContext();
            var users = await dbContext.StudentAccounts.ToListAsync();
            users.Should().HaveCount(1, "User should be created in database");
            users[0].Email.Should().Be("debug.test@example.com");
            users[0].FullName.Should().Be("Debug Test User");
        }
    }
}