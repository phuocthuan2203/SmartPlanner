using SmartPlanner.Application.DTOs;

namespace SmartPlanner.Application.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDTO> BuildDashboardAsync(Guid studentId);
        Task<bool> MarkTaskDoneFromDashboardAsync(Guid taskId, Guid studentId);
    }
}