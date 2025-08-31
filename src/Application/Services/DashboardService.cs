using AutoMapper;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Infrastructure.Repositories;

namespace SmartPlanner.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public DashboardService(ITaskRepository taskRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        public async Task<DashboardDTO> BuildDashboardAsync(Guid studentId)
        {
            // Get all tasks for the student
            var allTasks = await _taskRepository.GetByStudentIdAsync(studentId);
            var taskDTOs = _mapper.Map<IEnumerable<TaskDTO>>(allTasks).ToList();

            // Filter pending tasks (is_done = false)
            var pendingTasks = taskDTOs.Where(t => !t.IsDone).ToList();

            // Get today's date for comparison
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Separate into Today's Tasks (due today) vs Upcoming Tasks (due after today)
            var todayTasks = pendingTasks
                .Where(t => t.Deadline.Date == today)
                .OrderBy(t => t.Deadline)
                .ToList();

            var upcomingTasks = pendingTasks
                .Where(t => t.Deadline.Date >= tomorrow)
                .OrderBy(t => t.Deadline)
                .ToList();

            // Calculate progress statistics
            var totalTasks = taskDTOs.Count;
            var completedTasks = taskDTOs.Count(t => t.IsDone);
            var progressPercentage = totalTasks > 0 ? Math.Round((double)completedTasks / totalTasks * 100, 1) : 0;

            return new DashboardDTO
            {
                TodayTasks = todayTasks,
                UpcomingTasks = upcomingTasks,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                ProgressPercentage = progressPercentage,
                HasNoTasks = totalTasks == 0
            };
        }

        public async Task<bool> MarkTaskDoneFromDashboardAsync(Guid taskId, Guid studentId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, studentId);
            if (task == null)
            {
                return false;
            }

            task.IsDone = true;
            await _taskRepository.UpdateAsync(task);
            return true;
        }
    }
}