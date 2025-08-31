using Moq;
using Xunit;
using FluentAssertions;
using AutoMapper;
using SmartPlanner.Application.Services;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Domain.Entities;
using SmartPlanner.Infrastructure.Repositories;
using Task = System.Threading.Tasks.Task;

namespace SmartPlanner.Tests.Unit.Services
{
    public class DashboardServiceTests : TestBase
    {
        private readonly Mock<ITaskRepository> _mockTaskRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DashboardService _dashboardService;

        public DashboardServiceTests()
        {
            _mockTaskRepo = MockRepository.Create<ITaskRepository>();
            _mockMapper = MockRepository.Create<IMapper>();
            _dashboardService = new DashboardService(_mockTaskRepo.Object, _mockMapper.Object);
        }

        private static Domain.Entities.Task CreateTaskWithDeadline(Guid studentId, DateTime deadline, bool isDone)
        {
            var task = TestDataFactory.CreateValidTask(studentId);
            task.Deadline = deadline;
            task.IsDone = isDone;
            return task;
        }

        [Fact]
        public async Task BuildDashboardAsync_WithMixedTasks_ShouldCategorizeProperly()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var today = DateTime.Today;
            
            var testTasks = new List<Domain.Entities.Task>
            {
                // Today's incomplete task
                CreateTaskWithDeadline(studentId, today, false),
                // Future incomplete task
                CreateTaskWithDeadline(studentId, today.AddDays(3), false),
                // Completed task (should be excluded from dashboard lists)
                CreateTaskWithDeadline(studentId, today, true),
                // Past incomplete task (overdue)
                CreateTaskWithDeadline(studentId, today.AddDays(-1), false)
            };

            var taskDTOs = testTasks.Select(t => new TaskDTO
            {
                Id = t.Id,
                StudentId = t.StudentId,
                Title = t.Title,
                Deadline = t.Deadline,
                IsDone = t.IsDone
            }).ToList();

            _mockTaskRepo
                .Setup(x => x.GetByStudentIdAsync(studentId, null))
                .ReturnsAsync(testTasks);
                
            _mockMapper
                .Setup(x => x.Map<IEnumerable<TaskDTO>>(testTasks))
                .Returns(taskDTOs);

            // Act
            var result = await _dashboardService.BuildDashboardAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.TodayTasks.Should().HaveCount(1);
            result.UpcomingTasks.Should().HaveCount(1);
            result.TotalTasks.Should().Be(4);
            result.CompletedTasks.Should().Be(1);
            result.ProgressPercentage.Should().Be(25.0);
            
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
            var emptyTasks = new List<Domain.Entities.Task>();
            var emptyTaskDTOs = new List<TaskDTO>();

            _mockTaskRepo
                .Setup(x => x.GetByStudentIdAsync(studentId, null))
                .ReturnsAsync(emptyTasks);
                
            _mockMapper
                .Setup(x => x.Map<IEnumerable<TaskDTO>>(emptyTasks))
                .Returns(emptyTaskDTOs);

            // Act
            var result = await _dashboardService.BuildDashboardAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.TodayTasks.Should().BeEmpty();
            result.UpcomingTasks.Should().BeEmpty();
            result.TotalTasks.Should().Be(0);
            result.CompletedTasks.Should().Be(0);
            result.ProgressPercentage.Should().Be(0);
            result.HasNoTasks.Should().BeTrue();
        }

        [Fact]
        public async Task BuildDashboardAsync_WithOnlyCompletedTasks_ShouldReturnEmptyTaskLists()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var completedTasks = new List<Domain.Entities.Task>
            {
                CreateTaskWithDeadline(studentId, DateTime.Today, true),
                CreateTaskWithDeadline(studentId, DateTime.Today.AddDays(1), true)
            };

            var taskDTOs = completedTasks.Select(t => new TaskDTO
            {
                Id = t.Id,
                StudentId = t.StudentId,
                Title = t.Title,
                Deadline = t.Deadline,
                IsDone = t.IsDone
            }).ToList();

            _mockTaskRepo
                .Setup(x => x.GetByStudentIdAsync(studentId, null))
                .ReturnsAsync(completedTasks);
                
            _mockMapper
                .Setup(x => x.Map<IEnumerable<TaskDTO>>(completedTasks))
                .Returns(taskDTOs);

            // Act
            var result = await _dashboardService.BuildDashboardAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.TodayTasks.Should().BeEmpty();
            result.UpcomingTasks.Should().BeEmpty();
            result.TotalTasks.Should().Be(2);
            result.CompletedTasks.Should().Be(2);
            result.ProgressPercentage.Should().Be(100.0);
            result.HasNoTasks.Should().BeFalse();
        }

        [Fact]
        public async Task BuildDashboardAsync_WithOnlyTodayTasks_ShouldReturnOnlyTodayTasks()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var today = DateTime.Today;
            var todayTasks = new List<Domain.Entities.Task>
            {
                CreateTaskWithDeadline(studentId, today, false),
                CreateTaskWithDeadline(studentId, today.AddHours(5), false)
            };

            var taskDTOs = todayTasks.Select(t => new TaskDTO
            {
                Id = t.Id,
                StudentId = t.StudentId,
                Title = t.Title,
                Deadline = t.Deadline,
                IsDone = t.IsDone
            }).ToList();

            _mockTaskRepo
                .Setup(x => x.GetByStudentIdAsync(studentId, null))
                .ReturnsAsync(todayTasks);
                
            _mockMapper
                .Setup(x => x.Map<IEnumerable<TaskDTO>>(todayTasks))
                .Returns(taskDTOs);

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
            var upcomingTasks = new List<Domain.Entities.Task>
            {
                CreateTaskWithDeadline(studentId, today.AddDays(2), false),
                CreateTaskWithDeadline(studentId, today.AddDays(7), false)
            };

            var taskDTOs = upcomingTasks.Select(t => new TaskDTO
            {
                Id = t.Id,
                StudentId = t.StudentId,
                Title = t.Title,
                Deadline = t.Deadline,
                IsDone = t.IsDone
            }).ToList();

            _mockTaskRepo
                .Setup(x => x.GetByStudentIdAsync(studentId, null))
                .ReturnsAsync(upcomingTasks);
                
            _mockMapper
                .Setup(x => x.Map<IEnumerable<TaskDTO>>(upcomingTasks))
                .Returns(taskDTOs);

            // Act
            var result = await _dashboardService.BuildDashboardAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.TodayTasks.Should().BeEmpty();
            result.UpcomingTasks.Should().HaveCount(2);
            result.UpcomingTasks.All(t => t.Deadline.Date > today).Should().BeTrue();
        }

        [Fact]
        public async Task MarkTaskDoneFromDashboardAsync_WithValidTask_ShouldReturnTrue()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var studentId = Guid.NewGuid();
            var existingTask = TestDataFactory.CreateValidTask(studentId);
            existingTask.Id = taskId;
            existingTask.IsDone = false;

            _mockTaskRepo
                .Setup(x => x.GetByIdAsync(taskId, studentId))
                .ReturnsAsync(existingTask);
                
            _mockTaskRepo
                .Setup(x => x.UpdateAsync(existingTask))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _dashboardService.MarkTaskDoneFromDashboardAsync(taskId, studentId);

            // Assert
            result.Should().BeTrue();
            existingTask.IsDone.Should().BeTrue();
        }

        [Fact]
        public async Task MarkTaskDoneFromDashboardAsync_WithNonExistentTask_ShouldReturnFalse()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var studentId = Guid.NewGuid();

            _mockTaskRepo
                .Setup(x => x.GetByIdAsync(taskId, studentId))
                .ReturnsAsync((Domain.Entities.Task?)null);

            // Act
            var result = await _dashboardService.MarkTaskDoneFromDashboardAsync(taskId, studentId);

            // Assert
            result.Should().BeFalse();
        }
    }
}