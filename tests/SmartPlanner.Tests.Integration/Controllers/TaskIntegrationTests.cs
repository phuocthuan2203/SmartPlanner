using System.Net;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace SmartPlanner.Tests.Integration.Controllers
{
    public class TaskIntegrationTests : IntegrationTestBase
    {
        private readonly string _testEmail = "tasks.test@example.com";
        private readonly string _testFullName = "Tasks Test User";
        private readonly string _testPassword = "TasksPassword123!";

        public TaskIntegrationTests(IntegrationTestFixture factory) : base(factory) { }

        private async Task AuthenticateUserAsync()
        {
            await RegisterUserAsync(_testEmail, _testFullName, _testPassword);
        }

        [Fact]
        public async Task CreateTask_WithValidData_ShouldCreateAndRedirectToTaskList()
        {
            // Arrange
            await AuthenticateUserAsync();

            // Get create task page
            var createPageResponse = await Client.GetAsync("/Task/Create");
            createPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var createDocument = await GetDocumentAsync(createPageResponse);
            var token = GetAntiForgeryToken(createDocument);

            var taskData = new Dictionary<string, string>
            {
                ["Title"] = "Integration Test Task",
                ["Description"] = "This is a test task created during integration testing",
                ["Deadline"] = DateTime.Now.AddDays(7).ToString("yyyy-MM-ddTHH:mm"),
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync("/Task/Create", CreateFormContent(taskData));

            // Assert
            // Should redirect to task list on successful creation
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location?.ToString().Should().Contain("Task");

            // Verify task exists in database
            using var dbContext = GetDbContext();
            var createdTask = await dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Title == "Integration Test Task");
            createdTask.Should().NotBeNull();
            createdTask!.Description.Should().Be("This is a test task created during integration testing");
            createdTask.IsDone.Should().BeFalse();
        }

        [Fact]
        public async Task GetTasks_WithAuthenticatedUser_ShouldShowTaskList()
        {
            // Arrange
            await AuthenticateUserAsync();

            // Create some test tasks first
            await CreateTestTaskAsync("Task 1", DateTime.Now.AddDays(1));
            await CreateTestTaskAsync("Task 2", DateTime.Now.AddDays(2));

            // Act
            var response = await Client.GetAsync("/Task");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Should contain both created tasks
            responseContent.Should().Contain("Task 1");
            responseContent.Should().Contain("Task 2");
        }

        [Fact]
        public async Task EditTask_WithValidData_ShouldUpdateAndRedirectToTaskList()
        {
            // Arrange
            await AuthenticateUserAsync();
            var taskId = await CreateTestTaskAsync("Original Title", DateTime.Now.AddDays(5));

            // Get edit task page
            var editPageResponse = await Client.GetAsync($"/Task/Edit/{taskId}");
            editPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var editDocument = await GetDocumentAsync(editPageResponse);
            var token = GetAntiForgeryToken(editDocument);

            var updateData = new Dictionary<string, string>
            {
                ["Id"] = taskId.ToString(),
                ["Title"] = "Updated Title",
                ["Description"] = "Updated description",
                ["Deadline"] = DateTime.Now.AddDays(10).ToString("yyyy-MM-ddTHH:mm"),
                ["IsDone"] = "true",
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync("/Task/Edit", CreateFormContent(updateData));

            // Assert
            // Should redirect to task list on successful update
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location?.ToString().Should().Contain("Task");

            // Verify task is updated in database
            using var dbContext = GetDbContext();
            var updatedTask = await dbContext.Tasks.FindAsync(taskId);
            updatedTask.Should().NotBeNull();
            updatedTask!.Title.Should().Be("Updated Title");
            updatedTask.Description.Should().Be("Updated description");
            updatedTask.IsDone.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteTask_WithValidId_ShouldRemoveTaskAndReturnSuccess()
        {
            // Arrange
            await AuthenticateUserAsync();
            var taskId = await CreateTestTaskAsync("Task to Delete", DateTime.Now.AddDays(3));

            // Get the task list page to get anti-forgery token
            var taskListResponse = await Client.GetAsync("/Task");
            var taskListDocument = await GetDocumentAsync(taskListResponse);
            var token = GetAntiForgeryToken(taskListDocument);

            var deleteData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync($"/Task/Delete/{taskId}", CreateFormContent(deleteData));

            // Assert
            // Should return JSON success response
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("success");

            // Verify task no longer exists in database
            using var dbContext = GetDbContext();
            var deletedTask = await dbContext.Tasks.FindAsync(taskId);
            deletedTask.Should().BeNull();
        }

        [Fact]
        public async Task ToggleTaskStatus_WithValidId_ShouldUpdateStatusAndReturnSuccess()
        {
            // Arrange
            await AuthenticateUserAsync();
            var taskId = await CreateTestTaskAsync("Task to Toggle", DateTime.Now.AddDays(1));

            // Get the task list page to get anti-forgery token
            var taskListResponse = await Client.GetAsync("/Task");
            var taskListDocument = await GetDocumentAsync(taskListResponse);
            var token = GetAntiForgeryToken(taskListDocument);

            var toggleData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync($"/Task/ToggleStatus/{taskId}", CreateFormContent(toggleData));

            // Assert
            // Should return JSON success response
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("success");

            // Verify task status is toggled in database
            using var dbContext = GetDbContext();
            var toggledTask = await dbContext.Tasks.FindAsync(taskId);
            toggledTask.Should().NotBeNull();
            toggledTask!.IsDone.Should().BeTrue(); // Should be toggled from false to true
        }

        [Fact]
        public async Task TaskOperations_WithoutAuthentication_ShouldRedirectToLogin()
        {
            // Arrange - Use a fresh client without authentication
            var unauthenticatedClient = Factory.CreateClient();

            // Act & Assert
            // Test various endpoints return redirect to login without authentication
            var createResponse = await unauthenticatedClient.GetAsync("/Task/Create");
            createResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            createResponse.Headers.Location?.ToString().Should().Contain("Authentication/Login");

            var indexResponse = await unauthenticatedClient.GetAsync("/Task");
            indexResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            indexResponse.Headers.Location?.ToString().Should().Contain("Authentication/Login");

            var editResponse = await unauthenticatedClient.GetAsync($"/Task/Edit/{Guid.NewGuid()}");
            editResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            editResponse.Headers.Location?.ToString().Should().Contain("Authentication/Login");
        }

        [Fact]
        public async Task CreateTask_WithInvalidData_ShouldShowValidationErrors()
        {
            // Arrange
            await AuthenticateUserAsync();

            var createPageResponse = await Client.GetAsync("/Task/Create");
            var createDocument = await GetDocumentAsync(createPageResponse);
            var token = GetAntiForgeryToken(createDocument);

            var invalidData = new Dictionary<string, string>
            {
                ["Title"] = "", // Required field empty
                ["Description"] = new string('x', 1001), // Too long
                ["Deadline"] = "", // Required field empty
                ["__RequestVerificationToken"] = token
            };

            // Act
            var response = await Client.PostAsync("/Task/Create", CreateFormContent(invalidData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Should contain validation error messages
            responseContent.Should().Contain("required");
            responseContent.Should().Contain("1000 characters");
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
    }
}