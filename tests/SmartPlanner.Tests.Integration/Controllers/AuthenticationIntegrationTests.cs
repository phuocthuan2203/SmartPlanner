using System.Net;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace SmartPlanner.Tests.Integration.Controllers
{
    public class AuthenticationIntegrationTests : IntegrationTestBase
    {
        public AuthenticationIntegrationTests(IntegrationTestFixture factory) : base(factory) { }

        [Fact]
        public async Task Register_WithValidData_ShouldCreateUserAndRedirectToDashboard()
        {
            // Arrange
            var email = "integration.test@example.com";
            var fullName = "Integration Test User";
            var password = "SecurePassword123!";

            // Act
            var result = await RegisterUserAsync(email, fullName, password);

            // Assert
            result.Should().Be("success");

            // Verify user exists in database
            using var dbContext = GetDbContext();
            var createdUser = await dbContext.StudentAccounts
                .FirstOrDefaultAsync(u => u.Email == email);
            createdUser.Should().NotBeNull();
            createdUser!.FullName.Should().Be(fullName);
            createdUser.Email.Should().Be(email);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldShowError()
        {
            // Arrange
            var email = "duplicate@example.com";
            var fullName = "First User";
            var password = "Password123!";

            // Register user first time - this will redirect to dashboard
            await RegisterUserAsync(email, fullName, password);

            // Create a new client to avoid session conflicts
            var newClient = Factory.CreateClient();

            // Get register page for second attempt
            var registerPageResponse = await newClient.GetAsync("/Authentication/Register");
            var registerDocument = await GetDocumentAsync(registerPageResponse);
            var token = GetAntiForgeryToken(registerDocument);

            // Attempt to register same email again
            var duplicateData = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["FullName"] = "Second User",
                ["Password"] = password,
                ["ConfirmPassword"] = password,
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await newClient.PostAsync("/Authentication/Register", CreateFormContent(duplicateData));

            // Assert
            // Should return to register page with error (not redirect)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Account with this email already exists");
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldRedirectToDashboard()
        {
            // Arrange
            var email = "login.test@example.com";
            var fullName = "Login Test User";
            var password = "LoginPassword123!";

            // First register a user
            await RegisterUserAsync(email, fullName, password);

            // Clear any existing session by creating new client
            var newClient = Factory.CreateClient();

            // Get login page
            var loginPageResponse = await newClient.GetAsync("/Authentication/Login");
            var loginDocument = await GetDocumentAsync(loginPageResponse);
            var token = GetAntiForgeryToken(loginDocument);

            var loginData = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Password"] = password,
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await newClient.PostAsync("/Authentication/Login", CreateFormContent(loginData));

            // Assert
            // Should redirect to dashboard on successful login
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location?.ToString().Should().Contain("Dashboard");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldShowError()
        {
            // Arrange
            var loginPageResponse = await Client.GetAsync("/Authentication/Login");
            var loginDocument = await GetDocumentAsync(loginPageResponse);
            var token = GetAntiForgeryToken(loginDocument);

            var loginData = new Dictionary<string, string>
            {
                ["Email"] = "nonexistent@example.com",
                ["Password"] = "WrongPassword123!",
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync("/Authentication/Login", CreateFormContent(loginData));

            // Assert
            // Should return to login page with error (not redirect)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task Register_WithInvalidData_ShouldShowValidationErrors()
        {
            // Arrange
            var registerPageResponse = await Client.GetAsync("/Authentication/Register");
            var registerDocument = await GetDocumentAsync(registerPageResponse);
            var token = GetAntiForgeryToken(registerDocument);

            var invalidData = new Dictionary<string, string>
            {
                ["Email"] = "invalid-email", // Invalid email format
                ["FullName"] = "A", // Too short
                ["Password"] = "123", // Too short
                ["ConfirmPassword"] = "456", // Doesn't match
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync("/Authentication/Register", CreateFormContent(invalidData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Should contain validation error messages
            responseContent.Should().Contain("valid email");
            responseContent.Should().Contain("at least 6 characters");
            responseContent.Should().Contain("do not match");
        }

        [Fact]
        public async Task Logout_ShouldClearSessionAndRedirectToLogin()
        {
            // Arrange
            var email = "logout.test@example.com";
            var fullName = "Logout Test User";
            var password = "LogoutPassword123!";

            // Register and login user
            await RegisterUserAsync(email, fullName, password);

            // Get a page that requires authentication to verify we're logged in
            var dashboardResponse = await Client.GetAsync("/Dashboard");
            dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Get logout form token (if needed)
            var logoutData = new Dictionary<string, string>();

            // Act
            var logoutResponse = await Client.PostAsync("/Authentication/Logout", CreateFormContent(logoutData));

            // Assert
            // Should redirect to login page
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            logoutResponse.Headers.Location?.ToString().Should().Contain("Authentication/Login");

            // Verify session is cleared by trying to access dashboard
            var newDashboardResponse = await Client.GetAsync("/Dashboard");
            newDashboardResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            newDashboardResponse.Headers.Location?.ToString().Should().Contain("Authentication/Login");
        }
    }
}