# SmartPlanner Testing Framework Setup & Implementation Plan (AI Agent Instructions)

## **Project Context**

Current project structure:
```
~/code/SmartPlanner/
├── src/
│   ├── Application/
│   ├── Controllers/
│   ├── Domain/
│   ├── Infrastructure/
│   ├── Views/
│   ├── SmartPlanner.csproj
│   └── ... (all existing source files)
├── tests/ (empty, needs to be created)
├── SmartPlanner.sln (already includes src project)
└── README.md
```

## **Phase 1: Test Project Setup Commands**

Execute these commands in sequence from `~/code/SmartPlanner` directory:

```bash
# Step 1: Create test project directories
cd tests

# Create Unit Test Project
mkdir SmartPlanner.Tests.Unit
cd SmartPlanner.Tests.Unit
dotnet new xunit
dotnet add reference ../../src/SmartPlanner.csproj
cd ..

# Create Integration Test Project
mkdir SmartPlanner.Tests.Integration
cd SmartPlanner.Tests.Integration
dotnet new xunit
dotnet add reference ../../src/SmartPlanner.csproj
cd ..

# Create UI Test Project
mkdir SmartPlanner.Tests.UI
cd SmartPlanner.Tests.UI
dotnet new xunit
dotnet add reference ../../src/SmartPlanner.csproj
cd ..

# Return to project root
cd ..

# Step 2: Add test projects to solution
dotnet sln add tests/SmartPlanner.Tests.Unit/SmartPlanner.Tests.Unit.csproj
dotnet sln add tests/SmartPlanner.Tests.Integration/SmartPlanner.Tests.Integration.csproj
dotnet sln add tests/SmartPlanner.Tests.UI/SmartPlanner.Tests.UI.csproj

# Step 3: Add NuGet packages for Unit Tests
cd tests/SmartPlanner.Tests.Unit
dotnet add package Moq --version 4.20.69
dotnet add package FluentAssertions --version 6.12.0
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0

# Add packages for Integration Tests
cd ../SmartPlanner.Tests.Integration
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
dotnet add package FluentAssertions --version 6.12.0

# Add packages for UI Tests
cd ../SmartPlanner.Tests.UI
dotnet add package Selenium.WebDriver --version 4.15.0
dotnet add package Selenium.WebDriver.ChromeDriver --version 118.0.5993.7000
dotnet add package FluentAssertions --version 6.12.0

# Return to project root
cd ../..
```

## **Phase 2: Create Test Infrastructure Files**

### **File 1: TestBase.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Unit/TestBase.cs`

```csharp
using Moq;

namespace SmartPlanner.Tests.Unit
{
    public abstract class TestBase : IDisposable
    {
        protected MockRepository MockRepository { get; }

        protected TestBase()
        {
            MockRepository = new MockRepository(MockBehavior.Strict);
        }

        public virtual void Dispose()
        {
            MockRepository.VerifyAll();
        }
    }
}
```

### **File 2: TestDataFactory.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Unit/TestDataFactory.cs`

```csharp
using SmartPlanner.Domain.Entities;
using SmartPlanner.Application.DTOs;

namespace SmartPlanner.Tests.Unit
{
    public static class TestDataFactory
    {
        public static StudentAccount CreateValidStudentAccount()
        {
            return new StudentAccount
            {
                StudentId = Guid.NewGuid(),
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = "hashed_password_123",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Task CreateValidTask(Guid studentId, Guid? subjectId = null)
        {
            return new Task
            {
                TaskId = Guid.NewGuid(),
                StudentId = studentId,
                SubjectId = subjectId,
                Title = "Test Task",
                Description = "Test task description",
                Deadline = DateTime.UtcNow.AddDays(1),
                IsDone = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Subject CreateValidSubject(Guid studentId)
        {
            return new Subject
            {
                SubjectId = Guid.NewGuid(),
                StudentId = studentId,
                Name = "Mathematics",
                Description = "Advanced Mathematics Course"
            };
        }

        public static StudentRegisterDTO CreateValidRegisterDTO()
        {
            return new StudentRegisterDTO
            {
                Email = "newuser@example.com",
                FullName = "New Test User",
                Password = "validPassword123!"
            };
        }

        public static LoginDTO CreateValidLoginDTO()
        {
            return new LoginDTO
            {
                Email = "test@example.com",
                Password = "validPassword123!"
            };
        }

        public static TaskCreateDTO CreateValidTaskCreateDTO(Guid studentId)
        {
            return new TaskCreateDTO
            {
                StudentId = studentId,
                Title = "New Task",
                Deadline = DateTime.UtcNow.AddDays(7)
            };
        }

        public static TaskUpdateDTO CreateValidTaskUpdateDTO(Guid taskId)
        {
            return new TaskUpdateDTO
            {
                TaskId = taskId,
                Title = "Updated Task Title",
                Deadline = DateTime.UtcNow.AddDays(5),
                IsDone = false
            };
        }

        public static DashboardDTO CreateEmptyDashboardDTO()
        {
            return new DashboardDTO
            {
                TodayTasks = new List<TaskDTO>(),
                UpcomingTasks = new List<TaskDTO>()
            };
        }
    }
}
```

## **Phase 3: Implement Unit Test Files**

### **File 3: AuthenticationServiceTests.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Unit/Services/AuthenticationServiceTests.cs`

```csharp
using Moq;
using Xunit;
using FluentAssertions;
using SmartPlanner.Application.Services;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Domain.Entities;
using SmartPlanner.Domain;

namespace SmartPlanner.Tests.Unit.Services
{
    public class AuthenticationServiceTests : TestBase
    {
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<ISecurityService> _mockSecurityService;
        private readonly Mock<IValidationService> _mockValidationService;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            _mockStudentRepo = MockRepository.Create<IStudentRepository>();
            _mockSecurityService = MockRepository.Create<ISecurityService>();
            _mockValidationService = MockRepository.Create<IValidationService>();
            
            _authService = new AuthenticationService(
                _mockStudentRepo.Object,
                _mockSecurityService.Object,
                _mockValidationService.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldReturnSuccessResponse()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDTO();
            
            _mockValidationService
                .Setup(x => x.ValidateRegister(registerDto))
                .Returns(new ValidationResult { IsValid = true });
            
            _mockStudentRepo
                .Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((StudentAccount?)null);
                
            _mockSecurityService
                .Setup(x => x.HashPassword(registerDto.Password))
                .Returns("hashed_password");
                
            _mockSecurityService
                .Setup(x => x.GenerateAuthToken(It.IsAny<Guid>()))
                .Returns("mock_jwt_token");
                
            _mockStudentRepo
                .Setup(x => x.SaveAsync(It.IsAny<StudentAccount>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Token.Should().Be("mock_jwt_token");
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ShouldReturnFailureResponse()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDTO();
            var existingStudent = TestDataFactory.CreateValidStudentAccount();
            
            _mockValidationService
                .Setup(x => x.ValidateRegister(registerDto))
                .Returns(new ValidationResult { IsValid = true });
                
            _mockStudentRepo
                .Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingStudent);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("already exists");
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidData_ShouldReturnFailureResponse()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDTO();
            var validationResult = new ValidationResult 
            { 
                IsValid = false, 
                Errors = new List<string> { "Email format is invalid" } 
            };
            
            _mockValidationService
                .Setup(x => x.ValidateRegister(registerDto))
                .Returns(validationResult);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Email format is invalid");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessResponse()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDTO();
            var existingStudent = TestDataFactory.CreateValidStudentAccount();
            
            _mockValidationService
                .Setup(x => x.ValidateLogin(loginDto))
                .Returns(new ValidationResult { IsValid = true });
                
            _mockStudentRepo
                .Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(existingStudent);
                
            _mockSecurityService
                .Setup(x => x.VerifyPassword(loginDto.Password, existingStudent.PasswordHash))
                .Returns(true);
                
            _mockSecurityService
                .Setup(x => x.GenerateAuthToken(existingStudent.StudentId))
                .Returns("mock_jwt_token");

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Token.Should().Be("mock_jwt_token");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailureResponse()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDTO();
            var existingStudent = TestDataFactory.CreateValidStudentAccount();
            
            _mockValidationService
                .Setup(x => x.ValidateLogin(loginDto))
                .Returns(new ValidationResult { IsValid = true });
                
            _mockStudentRepo
                .Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(existingStudent);
                
            _mockSecurityService
                .Setup(x => x.VerifyPassword(loginDto.Password, existingStudent.PasswordHash))
                .Returns(false);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid credentials");
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentUser_ShouldReturnFailureResponse()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDTO();
            
            _mockValidationService
                .Setup(x => x.ValidateLogin(loginDto))
                .Returns(new ValidationResult { IsValid = true });
                
            _mockStudentRepo
                .Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((StudentAccount?)null);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid credentials");
        }
    }
}
```

### **File 4: TaskServiceTests.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Unit/Services/TaskServiceTests.cs`

```csharp
using Moq;
using Xunit;
using FluentAssertions;
using SmartPlanner.Application.Services;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Domain.Entities;
using SmartPlanner.Domain;

namespace SmartPlanner.Tests.Unit.Services
{
    public class TaskServiceTests : TestBase
    {
        private readonly Mock<ITaskRepository> _mockTaskRepo;
        private readonly Mock<IValidationService> _mockValidationService;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _mockTaskRepo = MockRepository.Create<ITaskRepository>();
            _mockValidationService = MockRepository.Create<IValidationService>();
            _taskService = new TaskService(_mockTaskRepo.Object, _mockValidationService.Object);
        }

        [Fact]
        public async Task CreateTaskAsync_WithValidData_ShouldReturnTaskDTO()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var createDto = TestDataFactory.CreateValidTaskCreateDTO(studentId);

            _mockValidationService
                .Setup(x => x.ValidateTask(createDto))
                .Returns(new ValidationResult { IsValid = true });

            _mockTaskRepo
                .Setup(x => x.SaveAsync(It.IsAny<Task>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.CreateTaskAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(createDto.Title);
            result.IsDone.Should().BeFalse();
        }

        [Fact]
        public async Task CreateTaskAsync_WithInvalidData_ShouldThrowException()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var createDto = TestDataFactory.CreateValidTaskCreateDTO(studentId);
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { "Title is required" }
            };

            _mockValidationService
                .Setup(x => x.ValidateTask(createDto))
                .Returns(validationResult);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _taskService.CreateTaskAsync(createDto));
        }

        [Fact]
        public async Task GetTasksAsync_ForValidStudent_ShouldReturnTaskList()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var tasks = new List<Task>
            {
                TestDataFactory.CreateValidTask(studentId),
                TestDataFactory.CreateValidTask(studentId)
            };

            _mockTaskRepo
                .Setup(x => x.FindByStudentAsync(studentId))
                .ReturnsAsync(tasks);

            // Act
            var result = await _taskService.GetTasksAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(t => t.StudentId == studentId).Should().BeTrue();
        }

        [Fact]
        public async Task GetTasksAsync_ForNonExistentStudent_ShouldReturnEmptyList()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var emptyTasks = new List<Task>();

            _mockTaskRepo
                .Setup(x => x.FindByStudentAsync(studentId))
                .ReturnsAsync(emptyTasks);

            // Act
            var result = await _taskService.GetTasksAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateTaskAsync_WithValidData_ShouldReturnUpdatedTask()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var existingTask = TestDataFactory.CreateValidTask(studentId);
            var updateDto = TestDataFactory.CreateValidTaskUpdateDTO(existingTask.TaskId);

            _mockTaskRepo
                .Setup(x => x.FindByIdAsync(updateDto.TaskId))
                .ReturnsAsync(existingTask);

            _mockTaskRepo
                .Setup(x => x.SaveAsync(It.IsAny<Task>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.UpdateTaskAsync(updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(updateDto.Title);
            result.TaskId.Should().Be(updateDto.TaskId);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithNonExistentTask_ShouldThrowException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var updateDto = TestDataFactory.CreateValidTaskUpdateDTO(taskId);

            _mockTaskRepo
                .Setup(x => x.FindByIdAsync(updateDto.TaskId))
                .ReturnsAsync((Task?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _taskService.UpdateTaskAsync(updateDto));
        }

        [Fact]
        public async Task DeleteTaskAsync_WithValidId_ShouldDeleteTask()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var studentId = Guid.NewGuid();
            var existingTask = TestDataFactory.CreateValidTask(studentId);

            _mockTaskRepo
                .Setup(x => x.FindByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _mockTaskRepo
                .Setup(x => x.DeleteAsync(taskId))
                .Returns(Task.CompletedTask);

            // Act
            await _taskService.DeleteTaskAsync(taskId);

            // Assert - MockRepository.VerifyAll() in TestBase will verify the calls
        }

        [Fact]
        public async Task DeleteTaskAsync_WithNonExistentId_ShouldThrowException()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockTaskRepo
                .Setup(x => x.FindByIdAsync(taskId))
                .ReturnsAsync((Task?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _taskService.DeleteTaskAsync(taskId));
        }
    }
}
```

### **File 5: DashboardServiceTests.cs**
Create: `~/code/SmartPlanner/tests/SmartPlanner.Tests.Unit/Services/DashboardServiceTests.cs`

```csharp
using Moq;
using Xunit;
using FluentAssertions;
using SmartPlanner.Application.Services;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Domain.Entities;

namespace SmartPlanner.Tests.Unit.Services
{
    public class DashboardServiceTests : TestBase
    {
        private readonly Mock<ITaskRepository> _mockTaskRepo;
        private readonly DashboardService _dashboardService;

        public DashboardServiceTests()
        {
            _mockTaskRepo = MockRepository.Create<ITaskRepository>();
            _dashboardService = new DashboardService(_mockTaskRepo.Object);
        }

        [Fact]
        public async Task BuildDashboardAsync_WithMixedTasks_ShouldCategorizeProperly()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var today = DateTime.Today;
            
            var testTasks = new List<Task>
            {
                // Today's incomplete task
                TestDataFactory.CreateValidTask(studentId) with { Deadline = today, IsDone = false },
                // Future incomplete task
                TestDataFactory.CreateValidTask(studentId) with { Deadline = today.AddDays(3), IsDone = false },
                // Completed task (should be excluded)
                TestDataFactory.CreateValidTask(studentId) with { Deadline = today, IsDone = true },
                // Past incomplete task
                TestDataFactory.CreateValidTask(studentId) with { Deadline = today.AddDays(-1), IsDone = false }
            };

            _mockTaskRepo
                .Setup(x => x.FindByStudentAsync(studentId))
                .ReturnsAsync(testTasks);

            // Act
            var result = await _dashboardService.BuildDashboardAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.TodayTasks.Should().HaveCount(1);
            result.UpcomingTasks.Should().HaveCount(1);
            
            // Verify today's task
            var todayTask = result.TodayTasks.First();
            todayTask.Deadline.Date.Should().Be(today);
            todayTask.IsDone.Should().BeFalse();
            
            // Verify upcoming task
            var upcomingTask = result.UpcomingTasks.First();
            upcomingTask.Deadline.Date.Should().Be(today.AddDays(3));
            upcomingTask.IsDone.Should().BeFalse();
        }

        [Fact]
        public async Task BuildDashboardAsync_WithNoTasks_ShouldReturnEmptyDashboard()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var emptyTasks = new List<Task>();

            _mockTaskRepo
                .Setup(x => x.FindByStudentAsync(studentId))
                .ReturnsAsync(emptyTasks);

            // Act
            var result = await _dashboardService.BuildDashboardAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.TodayTasks.Should().BeEmpty();
            result.UpcomingTasks.Should().BeEmpty();
        }

        [Fact]
        public async Task BuildDashboardAsync_WithOnlyCompletedTasks_ShouldReturnEmptyDashboard()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var completedTasks = new List<Task>
            {
                TestDataFactory.CreateValidTask(studentId) with { IsDone = true },
                TestDataFactory.CreateValidTask(studentId) with { IsDone = true }
            };

            _mockTaskRepo
                .Setup(x => x.FindByStudentAsync(studentId))
                .ReturnsAsync(completedTasks);

            // Act
            var result = await _dashboardService.BuildDashboardAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.TodayTasks.Should().BeEmpty();
            result.UpcomingTasks.Should().BeEmpty();
        }

        [Fact]
        public async Task BuildDashboardAsync_WithOnlyTodayTasks_ShouldReturnOnlyTodayTasks()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var today = DateTime.Today;
            var todayTasks = new List<Task>
            {
                TestDataFactory.CreateValidTask(studentId) with { Deadline = today, IsDone = false },
                TestDataFactory.CreateValidTask(studentId) with { Deadline = today.AddHours(5), IsDone = false }
            };

            _mockTaskRepo
                .Setup(x => x.FindByStudentAsync(studentId))
                .ReturnsAsync(todayTasks);

            // Act
            var result = await _dashboardService.BuildDashboardAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.TodayTasks.Should().HaveCount(2);
            result.UpcomingTasks.Should().BeEmpty();
            result.TodayTasks.All(t => t.Deadline.Date == today).Should().BeTrue();
        }

        [Fact]
        public async Task BuildDashboardAsync_WithOnlyUpcomingTasks_ShouldReturnOnlyUpcomingTasks()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var today = DateTime.Today;
            var upcomingTasks = new List<Task>
            {
                TestDataFactory.CreateValidTask(studentId) with { Deadline = today.AddDays(2), IsDone = false },
                TestDataFactory.CreateValidTask(studentId) with { Deadline = today.AddDays(7), IsDone = false }
            };

            _mockTaskRepo
                .Setup(x => x.FindByStudentAsync(studentId))
                .ReturnsAsync(upcomingTasks);

            // Act
            var result = await _dashboardService.BuildDashboardAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.TodayTasks.Should().BeEmpty();
            result.UpcomingTasks.Should().HaveCount(2);
            result.UpcomingTasks.All(t => t.Deadline.Date > today).Should().BeTrue();
        }
    }
}
```

## **Phase 4: Create Directory Structure**

Execute command to create the Services directory:

```bash
cd ~/code/SmartPlanner/tests/SmartPlanner.Tests.Unit
mkdir Services
cd ../../../
```

## **Phase 5: Update .gitignore**

Update: `~/code/SmartPlanner/.gitignore`

```gitignore
# Build outputs
**/bin/
**/obj/
**/out/

# Test outputs
**/TestResults/
**/coverage/
**/*.trx
**/*.coverage
**/*.coveragexml
**/TestCoverage/

# IDE files
.vs/
.vscode/
*.suo
*.user
*.userosscache
*.sln.docstates

# Database files
*.db
*.sqlite
*.sqlite3

# Logs
*.log

# OS files
.DS_Store
Thumbs.db

# Package files
*.nupkg
*.snupkg

# Backup files
*.bak
```

## **Phase 6: Verification Commands**

Execute these commands to verify the setup:

```bash
# Build entire solution
dotnet build

# Run all unit tests
dotnet test tests/SmartPlanner.Tests.Unit/

# Run tests with verbose output
dotnet test tests/SmartPlanner.Tests.Unit/ --verbosity normal

# Run specific test class
dotnet test tests/SmartPlanner.Tests.Unit/ --filter "ClassName=AuthenticationServiceTests"

# Verify solution includes all projects
dotnet sln list
```

## **Phase 7: Commit Changes**

```bash
# Add all changes
git add .

# Commit with descriptive message
git commit -m "Add comprehensive unit testing framework

- Create Unit, Integration, and UI test projects with proper references
- Implement TestBase abstract class for consistent mock management
- Add TestDataFactory for standardized test data generation
- Create comprehensive unit tests for AuthenticationService (6 test cases)
- Create comprehensive unit tests for TaskService (7 test cases)  
- Create comprehensive unit tests for DashboardService (5 test cases)
- Configure Moq, FluentAssertions, and other testing dependencies
- Add proper .gitignore rules for test artifacts
- All tests follow AAA (Arrange-Act-Assert) pattern with proper mocking"

# Push to repository
git push origin main
```

## **Expected Final Structure**

```
~/code/SmartPlanner/
├── src/                                    # Existing source code
├── tests/
│   ├── SmartPlanner.Tests.Unit/
│   │   ├── Services/
│   │   │   ├── AuthenticationServiceTests.cs    # 6 test methods
│   │   │   ├── TaskServiceTests.cs              # 7 test methods  
│   │   │   └── DashboardServiceTests.cs         # 5 test methods
│   │   ├── TestBase.cs
│   │   ├── TestDataFactory.cs
│   │   └── SmartPlanner.Tests.Unit.csproj
│   ├── SmartPlanner.Tests.Integration/
│   │   └── SmartPlanner.Tests.Integration.csproj
│   └── SmartPlanner.Tests.UI/
│       └── SmartPlanner.Tests.UI.csproj
├── SmartPlanner.sln                        # Includes all projects
├── .gitignore                              # Updated
└── README.md
```

## **Test Coverage Summary**

- **AuthenticationService**: 6 tests covering registration success/failure, login success/failure, validation, and non-existent user scenarios
- **TaskService**: 7 tests covering CRUD operations, validation failures, and edge cases  
- **DashboardService**: 5 tests covering task categorization, empty states, and different task combinations
- **Total**: 18 comprehensive unit tests with proper mocking and assertions

All tests use strict mocking behavior, follow AAA pattern, and include proper cleanup through the TestBase class.
