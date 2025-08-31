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
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = "hashed_password_123",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Domain.Entities.Task CreateValidTask(Guid studentId, Guid? subjectId = null)
        {
            return new Domain.Entities.Task
            {
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
                StudentId = studentId,
                Name = "Mathematics",
                Description = "Advanced Mathematics Course",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static StudentRegisterDTO CreateValidRegisterDTO()
        {
            return new StudentRegisterDTO
            {
                Email = "newuser@example.com",
                FullName = "New Test User",
                Password = "validPassword123!",
                ConfirmPassword = "validPassword123!"
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

        public static TaskUpdateDTO CreateValidTaskUpdateDTO(Guid taskId, Guid studentId)
        {
            return new TaskUpdateDTO
            {
                Id = taskId,
                StudentId = studentId,
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