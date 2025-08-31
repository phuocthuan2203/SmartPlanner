using SmartPlanner.Application.DTOs;

namespace SmartPlanner.Application.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDTO>> GetTasksByStudentAsync(Guid studentId, TaskSearchDTO? search = null);
        Task<IEnumerable<TaskDTO>> GetTasksByStudentAndSubjectAsync(Guid studentId, Guid subjectId);
        Task<TaskDTO?> GetTaskByIdAsync(Guid taskId, Guid studentId);
        Task<DashboardDTO> GetDashboardDataAsync(Guid studentId);
        Task<TaskDTO> CreateTaskAsync(TaskCreateDTO dto);
        Task<TaskDTO> UpdateTaskAsync(TaskUpdateDTO dto);
        Task<bool> DeleteTaskAsync(Guid taskId, Guid studentId);
        Task<bool> ToggleTaskStatusAsync(Guid taskId, Guid studentId);
        Task<bool> TaskExistsAsync(Guid taskId, Guid studentId);
        Task<IEnumerable<TaskDTO>> GetTodayTasksAsync(Guid studentId);
        Task<IEnumerable<TaskDTO>> GetUpcomingTasksAsync(Guid studentId, int days = 7);
        Task<IEnumerable<TaskDTO>> GetOverdueTasksAsync(Guid studentId);
    }
}