using System.Net.Http;
using System.Text;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using SmartPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AngleSharp;
using AngleSharp.Html.Dom;

namespace SmartPlanner.Tests.Integration
{
    public abstract class IntegrationTestBase : IClassFixture<IntegrationTestFixture>
    {
        protected readonly HttpClient Client;
        protected readonly IntegrationTestFixture Factory;
        protected readonly IConfiguration Config;

        protected IntegrationTestBase(IntegrationTestFixture factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
            Config = Configuration.Default;
        }

        // Helper method to create form content for POST requests
        protected FormUrlEncodedContent CreateFormContent(Dictionary<string, string> formData)
        {
            return new FormUrlEncodedContent(formData);
        }

        // Helper method to parse HTML response and extract form data
        protected async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var document = await BrowsingContext.New(Config).OpenAsync(req => req.Content(content));
            return (IHtmlDocument)document;
        }

        // Helper method to extract anti-forgery token from form
        protected string GetAntiForgeryToken(IHtmlDocument document)
        {
            var tokenInput = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement;
            return tokenInput?.Value ?? string.Empty;
        }

        // Helper method to get fresh database context for verification
        protected SmartPlannerDbContext GetDbContext()
        {
            var scope = Factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<SmartPlannerDbContext>();
        }

        // Helper method to simulate user login and get session cookies
        protected async Task<string> LoginUserAsync(string email, string password)
        {
            // Get login page to extract anti-forgery token
            var loginPageResponse = await Client.GetAsync("/Authentication/Login");
            var loginDocument = await GetDocumentAsync(loginPageResponse);
            var token = GetAntiForgeryToken(loginDocument);

            // Submit login form
            var loginData = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Password"] = password,
                ["__RequestVerificationToken"] = token
            };

            var loginResponse = await Client.PostAsync("/Authentication/Login", CreateFormContent(loginData));
            
            // Extract session cookie or return success indicator
            return loginResponse.IsSuccessStatusCode ? "success" : "failed";
        }

        // Helper method to register a new user
        protected async Task<string> RegisterUserAsync(string email, string fullName, string password)
        {
            // Get register page to extract anti-forgery token
            var registerPageResponse = await Client.GetAsync("/Authentication/Register");
            var registerDocument = await GetDocumentAsync(registerPageResponse);
            var token = GetAntiForgeryToken(registerDocument);

            // Submit registration form
            var registerData = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["FullName"] = fullName,
                ["Password"] = password,
                ["ConfirmPassword"] = password,
                ["__RequestVerificationToken"] = token
            };

            var registerResponse = await Client.PostAsync("/Authentication/Register", CreateFormContent(registerData));
            return registerResponse.IsSuccessStatusCode ? "success" : "failed";
        }
    }
}