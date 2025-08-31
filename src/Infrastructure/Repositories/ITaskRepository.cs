using SmartPlanner.Application.DTOs;
using SmartPlanner.Domain.Entities;

namespace SmartPlanner.Infrastructure.Repositories
{
    public interface ITaskRepository
    {
        Task<IEnumerable<Domain.Entities.Task>> GetByStudentIdAsync(Guid studentId, TaskSearchDTO? search = null);
        Task<IEnumerable<Domain.Entities.Task>> GetByStudentAndSubjectAsync(Guid studentId, Guid subjectId);
        Task<Domain.Entities.Task?> GetByIdAsync(Guid id, Guid studentId);
        Task<Domain.Entities.Task?> GetByIdAsync(Guid id);
        Task<Domain.Entities.Task> CreateAsync(Domain.Entities.Task task);
        Task<Domain.Entities.Task> UpdateAsync(Domain.Entities.Task task);
        Task<bool> DeleteAsync(Guid id, Guid studentId);
        Task<bool> ExistsAsync(Guid id, Guid studentId);
        Task<IEnumerable<Domain.Entities.Task>> GetTodayTasksAsync(Guid studentId);
        Task<IEnumerable<Domain.Entities.Task>> GetUpcomingTasksAsync(Guid studentId, int days = 7);
        Task<IEnumerable<Domain.Entities.Task>> GetOverdueTasksAsync(Guid studentId);
        Task<int> GetTotalTasksCountAsync(Guid studentId);
        Task<int> GetCompletedTasksCountAsync(Guid studentId);
    }
}