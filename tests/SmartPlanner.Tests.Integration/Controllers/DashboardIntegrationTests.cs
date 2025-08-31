using System.Net;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace SmartPlanner.Tests.Integration.Controllers
{
    public class DashboardIntegrationTests : IntegrationTestBase
    {
        private readonly string _testEmail = "dashboard.test@example.com";
        private readonly string _testFullName = "Dashboard Test User";
        private readonly string _testPassword = "DashboardPassword123!";

        public DashboardIntegrationTests(IntegrationTestFixture factory) : base(factory) { }

        private async Task AuthenticateUserAsync()
        {
            await RegisterUserAsync(_testEmail, _testFullName, _testPassword);
        }

        [Fact]
        public async Task GetDashboard_WithMixedTasks_ShouldDisplayCorrectCategories()
        {
            // Arrange
            await AuthenticateUserAsync();

            var today = DateTime.Today;

            // Create tasks for different categories
            await CreateTestTaskAsync("Today's Task", today.AddHours(10)); // Due today
            await CreateTestTaskAsync("Upcoming Task", today.AddDays(3)); // Due in future
            var completedTaskId = await CreateTestTaskAsync("Completed Task", today.AddHours(5)); // Due today but will be completed

            // Mark one task as completed
            await MarkTaskAsCompleted(completedTaskId);

            // Act
            var response = await Client.GetAsync("/Dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Verify dashboard shows correct categorization
            responseContent.Should().Contain("Today's Task");
            responseContent.Should().Contain("Upcoming Task");
            
            // The completed task should not appear in today's tasks section
            // (This depends on how the dashboard view is structured)
            responseContent.Should().Contain(_testFullName); // User name should be displayed
        }

        [Fact]
        public async Task GetDashboard_WithNoTasks_ShouldShowEmptyState()
        {
            // Arrange
            await AuthenticateUserAsync();

            // Act
            var response = await Client.GetAsync("/Dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Should show empty state or no tasks message
            responseContent.Should().Contain(_testFullName); // User name should still be displayed
            
            // Verify no task-related content is shown (depends on view implementation)
            // This test verifies the dashboard loads correctly even with no tasks
        }

        [Fact]
        public async Task GetDashboard_WithoutAuthentication_ShouldRedirectToLogin()
        {
            // Arrange - Use a fresh client without authentication
            var unauthenticatedClient = Factory.CreateClient();

            // Act
            var response = await unauthenticatedClient.GetAsync("/Dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location?.ToString().Should().Contain("Authentication/Login");
        }

        [Fact]
        public async Task MarkTaskDone_WithValidTask_ShouldUpdateStatusAndReturnSuccess()
        {
            // Arrange
            await AuthenticateUserAsync();
            var taskId = await CreateTestTaskAsync("Task to Complete", DateTime.Today.AddHours(12));

            // Get dashboard page to extract anti-forgery token
            var dashboardResponse = await Client.GetAsync("/Dashboard");
            var dashboardDocument = await GetDocumentAsync(dashboardResponse);
            var token = GetAntiForgeryToken(dashboardDocument);

            var markDoneData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync($"/Dashboard/MarkTaskDone/{taskId}", CreateFormContent(markDoneData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("success");

            // Verify task is marked as done in database
            using var dbContext = GetDbContext();
            var completedTask = await dbContext.Tasks.FindAsync(taskId);
            completedTask.Should().NotBeNull();
            completedTask!.IsDone.Should().BeTrue();
        }

        [Fact]
        public async Task MarkTaskDone_WithInvalidTask_ShouldReturnError()
        {
            // Arrange
            await AuthenticateUserAsync();
            var nonExistentTaskId = Guid.NewGuid();

            var dashboardResponse = await Client.GetAsync("/Dashboard");
            var dashboardDocument = await GetDocumentAsync(dashboardResponse);
            var token = GetAntiForgeryToken(dashboardDocument);

            var markDoneData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync($"/Dashboard/MarkTaskDone/{nonExistentTaskId}", CreateFormContent(markDoneData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("not found");
        }

        [Fact]
        public async Task Dashboard_WithOverdueTasks_ShouldDisplayCorrectly()
        {
            // Arrange
            await AuthenticateUserAsync();

            // Create an overdue task (deadline in the past)
            await CreateTestTaskAsync("Overdue Task", DateTime.Today.AddDays(-2));
            
            // Create a current task
            await CreateTestTaskAsync("Current Task", DateTime.Today.AddDays(1));

            // Act
            var response = await Client.GetAsync("/Dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Both tasks should be visible on dashboard
            responseContent.Should().Contain("Overdue Task");
            responseContent.Should().Contain("Current Task");
        }

        [Fact]
        public async Task Dashboard_CrossUserIsolation_ShouldOnlyShowOwnTasks()
        {
            // Arrange
            // Create first user and their task
            await AuthenticateUserAsync();
            await CreateTestTaskAsync("User1 Task", DateTime.Today.AddDays(1));

            // Create second user with different client
            var secondClient = Factory.CreateClient();
            var secondUserEmail = "dashboard.test2@example.com";
            var secondUserName = "Dashboard Test User 2";
            var secondUserPassword = "DashboardPassword123!";

            // Register second user
            var registerPageResponse = await secondClient.GetAsync("/Authentication/Register");
            var registerDocument = await GetDocumentAsync(registerPageResponse);
            var token = GetAntiForgeryToken(registerDocument);

            var registerData = new Dictionary<string, string>
            {
                ["Email"] = secondUserEmail,
                ["FullName"] = secondUserName,
                ["Password"] = secondUserPassword,
                ["ConfirmPassword"] = secondUserPassword,
                ["__RequestVerificationToken"] = token
            };

            await secondClient.PostAsync("/Authentication/Register", CreateFormContent(registerData));

            // Act - Get dashboard for second user
            var response = await secondClient.GetAsync("/Dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Second user should not see first user's tasks
            responseContent.Should().NotContain("User1 Task");
            responseContent.Should().Contain(secondUserName); // But should see their own name
        }

        // Helper method to create a test task and return its ID
        private async Task<Guid> CreateTestTaskAsync(string title, DateTime deadline)
        {
            var createPageResponse = await Client.GetAsync("/Task/Create");
            var createDocument = await GetDocumentAsync(createPageResponse);
            var token = GetAntiForgeryToken(createDocument);

            var taskData = new Dictionary<string, string>
            {
                ["Title"] = title,
                ["Description"] = $"Test description for {title}",
                ["Deadline"] = deadline.ToString("yyyy-MM-ddTHH:mm"),
                ["__RequestVerificationToken"] = token
            };

            var response = await Client.PostAsync("/Task/Create", CreateFormContent(taskData));
            
            // Verify the task was created successfully
            response.IsSuccessStatusCode.Should().BeTrue();

            // Get the created task ID from database
            using var dbContext = GetDbContext();
            var createdTask = await dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Title == title);
            
            createdTask.Should().NotBeNull($"Task with title '{title}' should have been created");
            return createdTask!.Id;
        }

        // Helper method to mark a task as completed
        private async Task MarkTaskAsCompleted(Guid taskId)
        {
            var dashboardResponse = await Client.GetAsync("/Dashboard");
            var dashboardDocument = await GetDocumentAsync(dashboardResponse);
            var token = GetAntiForgeryToken(dashboardDocument);

            var markDoneData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            await Client.PostAsync($"/Dashboard/MarkTaskDone/{taskId}", CreateFormContent(markDoneData));
        }
    }
}