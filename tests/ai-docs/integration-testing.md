# SmartPlanner Integration Testing Plan (AI Agent Instructions)

## **Phase 1: Project Analysis & Context Understanding**

### **Current Project State Assessment**
- **Project Structure**: `~/code/SmartPlanner/` with `src/` (main app) and `tests/` (testing projects)
- **Architecture**: Clean Architecture with Web → Application → Domain → Infrastructure layers
- **Existing Tests**: Unit tests implemented for core services (AuthenticationService, TaskService, DashboardService)
- **Technology Stack**: ASP.NET Core MVC, EF Core, SQLite/PostgreSQL, JWT authentication
- **Key Entities**: StudentAccount, Task, Subject with proper relationships
- **API Endpoints**: Authentication (/api/authentication), Tasks (/api/task), Dashboard (/api/dashboard)

### **Integration Testing Objectives**
- Test complete HTTP request/response cycles through all layers
- Validate end-to-end workflows (registration → login → task management → dashboard)
- Ensure proper JWT authentication and authorization flows
- Verify data persistence and retrieval through actual database operations
- Test API contract compliance and error handling scenarios

## **Phase 2: Integration Test Project Setup**

### **Step 1: Project Creation Commands**
Execute from `~/code/SmartPlanner/` directory:

```bash
# Navigate to tests directory (should already exist)
cd tests

# Verify SmartPlanner.Tests.Integration exists, if not create it
# (This should already be created from previous setup)
cd SmartPlanner.Tests.Integration

# Add additional integration testing packages
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
dotnet add package FluentAssertions --version 6.12.0
dotnet add package System.Text.Json --version 8.0.0

# Return to project root
cd ../..
```

## **Phase 3: Test Infrastructure Setup**

### **File 1: IntegrationTestFixture.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Integration/IntegrationTestFixture.cs`

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartPlanner.Infrastructure.Data;

namespace SmartPlanner.Tests.Integration
{
    public class IntegrationTestFixture : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // AI Agent: Remove the existing DbContext registration
                // Replace with in-memory database for testing isolation
                services.RemoveAll(typeof(DbContextOptions<SmartPlannerDbContext>));
                services.AddDbContext<SmartPlannerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid());
                });

                // AI Agent: Override any external service dependencies if needed
                // Example: Replace email service with mock implementation
                // services.Replace(ServiceDescriptor.Scoped<IEmailService, MockEmailService>());

                // AI Agent: Ensure database is created for each test
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SmartPlannerDbContext>();
                context.Database.EnsureCreated();
            });

            // AI Agent: Use test environment settings
            builder.UseEnvironment("Testing");
        }
    }
}
```

### **File 2: IntegrationTestBase.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Integration/IntegrationTestBase.cs`

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using SmartPlanner.Infrastructure.Data;
using SmartPlanner.Application.DTOs;

namespace SmartPlanner.Tests.Integration
{
    public abstract class IntegrationTestBase : IClassFixture<IntegrationTestFixture>
    {
        protected readonly HttpClient Client;
        protected readonly IntegrationTestFixture Factory;
        protected readonly JsonSerializerOptions JsonOptions;

        protected IntegrationTestBase(IntegrationTestFixture factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // AI Agent: Helper method to create JSON content for POST/PUT requests
        protected StringContent CreateJsonContent<T>(T data)
        {
            // AI Agent: Serialize object to JSON and create StringContent with proper headers
            var json = JsonSerializer.Serialize(data, JsonOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        // AI Agent: Helper method to deserialize response content
        protected async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            // AI Agent: Read response content and deserialize to specified type
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }

        // AI Agent: Helper method to add JWT token to request headers
        protected void AddAuthorizationHeader(string token)
        {
            // AI Agent: Add Bearer token to HttpClient default headers
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // AI Agent: Helper method to clean up authorization header
        protected void RemoveAuthorizationHeader()
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }

        // AI Agent: Helper method to get fresh database context for verification
        protected SmartPlannerDbContext GetDbContext()
        {
            var scope = Factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<SmartPlannerDbContext>();
        }
    }
}
```

## **Phase 4: Feature Integration Tests**

### **File 3: AuthenticationIntegrationTests.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Integration/Controllers/AuthenticationIntegrationTests.cs`

```csharp
using System.Net;
using Xunit;
using FluentAssertions;
using SmartPlanner.Application.DTOs;

namespace SmartPlanner.Tests.Integration.Controllers
{
    public class AuthenticationIntegrationTests : IntegrationTestBase
    {
        public AuthenticationIntegrationTests(IntegrationTestFixture factory) : base(factory) { }

        [Fact]
        public async Task Register_WithValidData_ShouldCreateUserAndReturnSuccess()
        {
            // Arrange
            var registerDto = new StudentRegisterDTO
            {
                // AI Agent: Create valid registration data
                Email = "integration.test@example.com",
                FullName = "Integration Test User",
                Password = "SecurePassword123!"
            };

            // Act
            var response = await Client.PostAsync("/api/authentication/register", 
                CreateJsonContent(registerDto));

            // Assert
            // AI Agent: Verify response status is successful
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // AI Agent: Deserialize response and verify success flag and token presence
            var authResponse = await DeserializeResponse<AuthResponse>(response);
            authResponse.Success.Should().BeTrue();
            authResponse.Token.Should().NotBeNullOrEmpty();
            authResponse.ErrorMessage.Should().BeNull();

            // AI Agent: Verify user exists in database
            using var dbContext = GetDbContext();
            var createdUser = await dbContext.StudentAccounts
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            createdUser.Should().NotBeNull();
            createdUser.FullName.Should().Be(registerDto.FullName);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnError()
        {
            // Arrange
            var registerDto = new StudentRegisterDTO
            {
                // AI Agent: Use same email as previous test to create conflict
                Email = "duplicate@example.com",
                FullName = "First User",
                Password = "Password123!"
            };

            // AI Agent: Register user first time
            await Client.PostAsync("/api/authentication/register", CreateJsonContent(registerDto));

            // AI Agent: Attempt to register same email again
            var duplicateDto = registerDto with { FullName = "Second User" };

            // Act
            var response = await Client.PostAsync("/api/authentication/register", 
                CreateJsonContent(duplicateDto));

            // Assert
            // AI Agent: Verify appropriate error response
            var authResponse = await DeserializeResponse<AuthResponse>(response);
            authResponse.Success.Should().BeFalse();
            authResponse.ErrorMessage.Should().Contain("already exists");
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            // AI Agent: First register a user
            var registerDto = new StudentRegisterDTO
            {
                Email = "login.test@example.com",
                FullName = "Login Test User",
                Password = "LoginPassword123!"
            };
            await Client.PostAsync("/api/authentication/register", CreateJsonContent(registerDto));

            var loginDto = new LoginDTO
            {
                // AI Agent: Use same credentials for login
                Email = registerDto.Email,
                Password = registerDto.Password
            };

            // Act
            var response = await Client.PostAsync("/api/authentication/login", 
                CreateJsonContent(loginDto));

            // Assert
            // AI Agent: Verify successful login response
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var authResponse = await DeserializeResponse<AuthResponse>(response);
            authResponse.Success.Should().BeTrue();
            authResponse.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnError()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await Client.PostAsync("/api/authentication/login", 
                CreateJsonContent(loginDto));

            // Assert
            // AI Agent: Verify error response for invalid credentials
            var authResponse = await DeserializeResponse<AuthResponse>(response);
            authResponse.Success.Should().BeFalse();
            authResponse.ErrorMessage.Should().Contain("Invalid credentials");
        }

        // AI Agent: Add test for logout endpoint when implemented
        // AI Agent: Add tests for invalid input data (missing fields, invalid email format, weak password)
    }
}
```

### **File 4: TasksIntegrationTests.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Integration/Controllers/TasksIntegrationTests.cs`

```csharp
using System.Net;
using Xunit;
using FluentAssertions;
using SmartPlanner.Application.DTOs;

namespace SmartPlanner.Tests.Integration.Controllers
{
    public class TasksIntegrationTests : IntegrationTestBase
    {
        private string _authToken;
        private Guid _studentId;

        public TasksIntegrationTests(IntegrationTestFixture factory) : base(factory) { }

        // AI Agent: Setup method to authenticate user before each test
        private async Task AuthenticateUser()
        {
            // AI Agent: Register and login a test user, store token and student ID
            var registerDto = new StudentRegisterDTO
            {
                Email = "tasks.test@example.com",
                FullName = "Tasks Test User",
                Password = "TasksPassword123!"
            };

            await Client.PostAsync("/api/authentication/register", CreateJsonContent(registerDto));

            var loginDto = new LoginDTO
            {
                Email = registerDto.Email,
                Password = registerDto.Password
            };

            var loginResponse = await Client.PostAsync("/api/authentication/login", 
                CreateJsonContent(loginDto));
            var authResponse = await DeserializeResponse<AuthResponse>(loginResponse);

            _authToken = authResponse.Token;
            AddAuthorizationHeader(_authToken);

            // AI Agent: Get student ID from database for use in tests
            using var dbContext = GetDbContext();
            var student = await dbContext.StudentAccounts
                .FirstAsync(s => s.Email == registerDto.Email);
            _studentId = student.StudentId;
        }

        [Fact]
        public async Task CreateTask_WithValidData_ShouldCreateAndReturnTask()
        {
            // Arrange
            await AuthenticateUser();

            var createTaskDto = new TaskCreateDTO
            {
                // AI Agent: Create valid task data
                StudentId = _studentId,
                Title = "Integration Test Task",
                Deadline = DateTime.UtcNow.AddDays(7)
            };

            // Act
            var response = await Client.PostAsync("/api/task", CreateJsonContent(createTaskDto));

            // Assert
            // AI Agent: Verify successful task creation
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var taskResponse = await DeserializeResponse<TaskDTO>(response);
            taskResponse.Title.Should().Be(createTaskDto.Title);
            taskResponse.IsDone.Should().BeFalse();

            // AI Agent: Verify task exists in database
            using var dbContext = GetDbContext();
            var createdTask = await dbContext.Tasks
                .FirstOrDefaultAsync(t => t.TaskId == taskResponse.TaskId);
            createdTask.Should().NotBeNull();
            createdTask.Title.Should().Be(createTaskDto.Title);
        }

        [Fact]
        public async Task GetTasks_WithAuthenticatedUser_ShouldReturnUserTasks()
        {
            // Arrange
            await AuthenticateUser();

            // AI Agent: Create some test tasks first
            var task1 = new TaskCreateDTO
            {
                StudentId = _studentId,
                Title = "Task 1",
                Deadline = DateTime.UtcNow.AddDays(1)
            };
            var task2 = new TaskCreateDTO
            {
                StudentId = _studentId,
                Title = "Task 2", 
                Deadline = DateTime.UtcNow.AddDays(2)
            };

            await Client.PostAsync("/api/task", CreateJsonContent(task1));
            await Client.PostAsync("/api/task", CreateJsonContent(task2));

            // Act
            var response = await Client.GetAsync($"/api/task/{_studentId}");

            // Assert
            // AI Agent: Verify response contains both created tasks
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var tasks = await DeserializeResponse<List<TaskDTO>>(response);
            tasks.Should().HaveCount(2);
            tasks.Should().Contain(t => t.Title == "Task 1");
            tasks.Should().Contain(t => t.Title == "Task 2");
        }

        [Fact]
        public async Task UpdateTask_WithValidData_ShouldUpdateAndReturnTask()
        {
            // Arrange
            await AuthenticateUser();

            // AI Agent: Create a task first
            var createTaskDto = new TaskCreateDTO
            {
                StudentId = _studentId,
                Title = "Original Title",
                Deadline = DateTime.UtcNow.AddDays(5)
            };
            var createResponse = await Client.PostAsync("/api/task", CreateJsonContent(createTaskDto));
            var createdTask = await DeserializeResponse<TaskDTO>(createResponse);

            var updateDto = new TaskUpdateDTO
            {
                // AI Agent: Update task data
                TaskId = createdTask.TaskId,
                Title = "Updated Title",
                Deadline = DateTime.UtcNow.AddDays(10),
                IsDone = true
            };

            // Act
            var response = await Client.PutAsync("/api/task", CreateJsonContent(updateDto));

            // Assert
            // AI Agent: Verify successful update
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedTask = await DeserializeResponse<TaskDTO>(response);
            updatedTask.Title.Should().Be("Updated Title");
            updatedTask.IsDone.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteTask_WithValidId_ShouldRemoveTask()
        {
            // Arrange
            await AuthenticateUser();

            // AI Agent: Create a task to delete
            var createTaskDto = new TaskCreateDTO
            {
                StudentId = _studentId,
                Title = "Task to Delete",
                Deadline = DateTime.UtcNow.AddDays(3)
            };
            var createResponse = await Client.PostAsync("/api/task", CreateJsonContent(createTaskDto));
            var createdTask = await DeserializeResponse<TaskDTO>(createResponse);

            // Act
            var response = await Client.DeleteAsync($"/api/task/{createdTask.TaskId}");

            // Assert
            // AI Agent: Verify successful deletion
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // AI Agent: Verify task no longer exists in database
            using var dbContext = GetDbContext();
            var deletedTask = await dbContext.Tasks
                .FirstOrDefaultAsync(t => t.TaskId == createdTask.TaskId);
            deletedTask.Should().BeNull();
        }

        [Fact]
        public async Task TaskOperations_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange - No authentication header set

            var createTaskDto = new TaskCreateDTO
            {
                StudentId = Guid.NewGuid(),
                Title = "Unauthorized Task",
                Deadline = DateTime.UtcNow.AddDays(1)
            };

            // Act & Assert
            // AI Agent: Test all endpoints return 401 without authentication
            var createResponse = await Client.PostAsync("/api/task", CreateJsonContent(createTaskDto));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var getResponse = await Client.GetAsync($"/api/task/{Guid.NewGuid()}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // AI Agent: Add similar tests for PUT and DELETE endpoints
        }
    }
}
```

### **File 5: DashboardIntegrationTests.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Integration/Controllers/DashboardIntegrationTests.cs`

```csharp
using System.Net;
using Xunit;
using FluentAssertions;
using SmartPlanner.Application.DTOs;

namespace SmartPlanner.Tests.Integration.Controllers
{
    public class DashboardIntegrationTests : IntegrationTestBase
    {
        private string _authToken;
        private Guid _studentId;

        public DashboardIntegrationTests(IntegrationTestFixture factory) : base(factory) { }

        private async Task AuthenticateUser()
        {
            // AI Agent: Implement authentication similar to TasksIntegrationTests
            // Register user, login, extract token and student ID
            var registerDto = new StudentRegisterDTO
            {
                Email = "dashboard.test@example.com",
                FullName = "Dashboard Test User",
                Password = "DashboardPassword123!"
            };

            await Client.PostAsync("/api/authentication/register", CreateJsonContent(registerDto));

            var loginDto = new LoginDTO { Email = registerDto.Email, Password = registerDto.Password };
            var loginResponse = await Client.PostAsync("/api/authentication/login", CreateJsonContent(loginDto));
            var authResponse = await DeserializeResponse<AuthResponse>(loginResponse);

            _authToken = authResponse.Token;
            AddAuthorizationHeader(_authToken);

            using var dbContext = GetDbContext();
            var student = await dbContext.StudentAccounts.FirstAsync(s => s.Email == registerDto.Email);
            _studentId = student.StudentId;
        }

        [Fact]
        public async Task GetDashboard_WithMixedTasks_ShouldCategorizeCorrectly()
        {
            // Arrange
            await AuthenticateUser();

            var today = DateTime.Today;

            // AI Agent: Create tasks for different categories
            var todayTask = new TaskCreateDTO
            {
                StudentId = _studentId,
                Title = "Today's Task",
                Deadline = today.AddHours(10) // Due today
            };

            var upcomingTask = new TaskCreateDTO
            {
                StudentId = _studentId,
                Title = "Upcoming Task",
                Deadline = today.AddDays(3) // Due in future
            };

            var completedTask = new TaskCreateDTO
            {
                StudentId = _studentId,
                Title = "Completed Task",
                Deadline = today.AddHours(5)
            };

            // AI Agent: Create all tasks
            await Client.PostAsync("/api/task", CreateJsonContent(todayTask));
            await Client.PostAsync("/api/task", CreateJsonContent(upcomingTask));
            var completedResponse = await Client.PostAsync("/api/task", CreateJsonContent(completedTask));
            var completedTaskDto = await DeserializeResponse<TaskDTO>(completedResponse);

            // AI Agent: Mark one task as completed
            var updateDto = new TaskUpdateDTO
            {
                TaskId = completedTaskDto.TaskId,
                Title = completedTaskDto.Title,
                Deadline = completedTaskDto.Deadline,
                IsDone = true
            };
            await Client.PutAsync("/api/task", CreateJsonContent(updateDto));

            // Act
            var response = await Client.GetAsync($"/api/dashboard/{_studentId}");

            // Assert
            // AI Agent: Verify dashboard categorization
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dashboard = await DeserializeResponse<DashboardDTO>(response);

            // AI Agent: Verify today's tasks (incomplete tasks due today)
            dashboard.TodayTasks.Should().HaveCount(1);
            dashboard.TodayTasks.First().Title.Should().Be("Today's Task");

            // AI Agent: Verify upcoming tasks (incomplete tasks due after today)
            dashboard.UpcomingTasks.Should().HaveCount(1);
            dashboard.UpcomingTasks.First().Title.Should().Be("Upcoming Task");

            // AI Agent: Verify completed task is not included in either category
            dashboard.TodayTasks.Should().NotContain(t => t.Title == "Completed Task");
            dashboard.UpcomingTasks.Should().NotContain(t => t.Title == "Completed Task");
        }

        [Fact]
        public async Task GetDashboard_WithNoTasks_ShouldReturnEmptyCategories()
        {
            // Arrange
            await AuthenticateUser();

            // Act
            var response = await Client.GetAsync($"/api/dashboard/{_studentId}");

            // Assert
            // AI Agent: Verify empty dashboard response
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dashboard = await DeserializeResponse<DashboardDTO>(response);
            dashboard.TodayTasks.Should().BeEmpty();
            dashboard.UpcomingTasks.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDashboard_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange - No authentication

            // Act
            var response = await Client.GetAsync($"/api/dashboard/{Guid.NewGuid()}");

            // Assert
            // AI Agent: Verify unauthorized response
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // AI Agent: Add test for cross-user data isolation (user A cannot see user B's dashboard)
        // AI Agent: Add test for past due tasks behavior if requirements specify
    }
}
```

## **Phase 5: Test Execution and CI Integration**

### **Test Execution Commands**
Execute from `~/code/SmartPlanner/` directory:

```bash
# Run all integration tests
dotnet test tests/SmartPlanner.Tests.Integration/ --verbosity normal

# Run with coverage collection
dotnet test tests/SmartPlanner.Tests.Integration/ --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test tests/SmartPlanner.Tests.Integration/ --filter "ClassName=AuthenticationIntegrationTests"

# Run all tests (unit + integration)
dotnet test

# Build and test together
dotnet build && dotnet test
```

### **CI Pipeline Integration Notes**

```yaml
# AI Agent: Add to CI pipeline (GitHub Actions, Azure DevOps, etc.)
# Example for .github/workflows/ci.yml

steps:
  - name: Run Integration Tests
    run: |
      dotnet test tests/SmartPlanner.Tests.Integration/ \
        --logger trx --results-directory TestResults/ \
        --collect:"XPlat Code Coverage"
  
  - name: Publish Test Results
    uses: dorny/test-reporter@v1
    if: always()
    with:
      name: Integration Tests
      path: TestResults/*.trx
      reporter: dotnet-trx
```

## **Phase 6: Validation and Quality Assurance**

### **Expected Test Coverage**
- **Authentication Flow**: Registration, login, duplicate handling, invalid credentials
- **Task Management**: CRUD operations, authorization, data validation
- **Dashboard**: Task categorization, empty states, unauthorized access
- **Security**: JWT token validation, cross-user data isolation
- **Error Handling**: Invalid inputs, missing authentication, resource not found

### **Success Criteria**
- All integration tests pass consistently
- Database state is properly isolated between tests
- HTTP status codes and response formats match API contracts
- Authentication and authorization work as expected
- Data persistence and retrieval function correctly through all layers

### **Post-Implementation Verification**
```bash
# AI Agent: Verify test results with these commands
dotnet test tests/SmartPlanner.Tests.Integration/ --logger "console;verbosity=detailed"

# AI Agent: Check that tests can run multiple times without interference
for i in {1..3}; do dotnet test tests/SmartPlanner.Tests.Integration/; done

# AI Agent: Verify integration with existing unit tests
dotnet test
```
