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
    public class TaskServiceTests : TestBase
    {
        private readonly Mock<ITaskRepository> _mockTaskRepo;
        private readonly Mock<ISubjectRepository> _mockSubjectRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _mockTaskRepo = MockRepository.Create<ITaskRepository>();
            _mockSubjectRepo = MockRepository.Create<ISubjectRepository>();
            _mockMapper = MockRepository.Create<IMapper>();
            
            _taskService = new TaskService(
                _mockTaskRepo.Object,
                _mockSubjectRepo.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task GetTasksByStudentAsync_WithValidStudentId_ShouldReturnTaskList()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var tasks = new List<Domain.Entities.Task>
            {
                TestDataFactory.CreateValidTask(studentId),
                TestDataFactory.CreateValidTask(studentId)
            };
            var taskDTOs = new List<TaskDTO>
            {
                new TaskDTO { Id = tasks[0].Id, StudentId = studentId, Title = tasks[0].Title },
                new TaskDTO { Id = tasks[1].Id, StudentId = studentId, Title = tasks[1].Title }
            };

            _mockTaskRepo
                .Setup(x => x.GetByStudentIdAsync(studentId, null))
                .ReturnsAsync(tasks);
                
            _mockMapper
                .Setup(x => x.Map<IEnumerable<TaskDTO>>(tasks))
                .Returns(taskDTOs);

            // Act
            var result = await _taskService.GetTasksByStudentAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(t => t.StudentId == studentId).Should().BeTrue();
        }

        [Fact]
        public async Task GetTasksByStudentAsync_WithNonExistentStudent_ShouldReturnEmptyList()
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
            var result = await _taskService.GetTasksByStudentAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateTaskAsync_WithValidData_ShouldReturnTaskDTO()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var createDto = TestDataFactory.CreateValidTaskCreateDTO(studentId);
            var createdTask = TestDataFactory.CreateValidTask(studentId);
            var taskDTO = new TaskDTO 
            { 
                Id = createdTask.Id, 
                StudentId = studentId, 
                Title = createDto.Title,
                IsDone = false
            };

            _mockMapper
                .Setup(x => x.Map<Domain.Entities.Task>(createDto))
                .Returns(createdTask);
                
            _mockTaskRepo
                .Setup(x => x.CreateAsync(createdTask))
                .ReturnsAsync(createdTask);
                
            _mockMapper
                .Setup(x => x.Map<TaskDTO>(createdTask))
                .Returns(taskDTO);

            // Act
            var result = await _taskService.CreateTaskAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(createDto.Title);
            result.IsDone.Should().BeFalse();
            result.StudentId.Should().Be(studentId);
        }

        [Fact]
        public async Task CreateTaskAsync_WithInvalidSubject_ShouldThrowException()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            var createDto = TestDataFactory.CreateValidTaskCreateDTO(studentId);
            createDto.SubjectId = subjectId;

            _mockSubjectRepo
                .Setup(x => x.ExistsAsync(subjectId, studentId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.CreateTaskAsync(createDto));
        }

        [Fact]
        public async Task CreateTaskAsync_WithPastDeadline_ShouldThrowException()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var createDto = TestDataFactory.CreateValidTaskCreateDTO(studentId);
            createDto.Deadline = DateTime.Now.AddDays(-1); // Past deadline

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.CreateTaskAsync(createDto));
        }

        [Fact]
        public async Task UpdateTaskAsync_WithValidData_ShouldReturnUpdatedTask()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var existingTask = TestDataFactory.CreateValidTask(studentId);
            existingTask.Id = taskId;
            var updateDto = TestDataFactory.CreateValidTaskUpdateDTO(taskId, studentId);
            var updatedTaskDTO = new TaskDTO 
            { 
                Id = taskId, 
                StudentId = studentId, 
                Title = updateDto.Title 
            };

            _mockTaskRepo
                .Setup(x => x.GetByIdAsync(taskId, studentId))
                .ReturnsAsync(existingTask);
                
            _mockTaskRepo
                .Setup(x => x.UpdateAsync(existingTask))
                .ReturnsAsync(existingTask);
                
            _mockMapper
                .Setup(x => x.Map<TaskDTO>(existingTask))
                .Returns(updatedTaskDTO);

            // Act
            var result = await _taskService.UpdateTaskAsync(updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(updateDto.Title);
            result.Id.Should().Be(taskId);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithNonExistentTask_ShouldThrowException()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var updateDto = TestDataFactory.CreateValidTaskUpdateDTO(taskId, studentId);

            _mockTaskRepo
                .Setup(x => x.GetByIdAsync(taskId, studentId))
                .ReturnsAsync((Domain.Entities.Task?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.UpdateTaskAsync(updateDto));
        }

        [Fact]
        public async Task DeleteTaskAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var studentId = Guid.NewGuid();

            _mockTaskRepo
                .Setup(x => x.DeleteAsync(taskId, studentId))
                .ReturnsAsync(true);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId, studentId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteTaskAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var studentId = Guid.NewGuid();

            _mockTaskRepo
                .Setup(x => x.DeleteAsync(taskId, studentId))
                .ReturnsAsync(false);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId, studentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ToggleTaskStatusAsync_WithValidTask_ShouldReturnTrue()
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
            var result = await _taskService.ToggleTaskStatusAsync(taskId, studentId);

            // Assert
            result.Should().BeTrue();
            existingTask.IsDone.Should().BeTrue(); // Should be toggled
        }
    }
}