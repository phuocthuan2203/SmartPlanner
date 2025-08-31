using AutoMapper;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Infrastructure.Repositories;

namespace SmartPlanner.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public TaskService(ITaskRepository taskRepository, ISubjectRepository subjectRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TaskDTO>> GetTasksByStudentAsync(Guid studentId, TaskSearchDTO? search = null)
        {
            var tasks = await _taskRepository.GetByStudentIdAsync(studentId, search);
            return _mapper.Map<IEnumerable<TaskDTO>>(tasks);
        }

        public async Task<IEnumerable<TaskDTO>> GetTasksByStudentAndSubjectAsync(Guid studentId, Guid subjectId)
        {
            var tasks = await _taskRepository.GetByStudentAndSubjectAsync(studentId, subjectId);
            return _mapper.Map<IEnumerable<TaskDTO>>(tasks);
        }

        public async Task<TaskDTO?> GetTaskByIdAsync(Guid taskId, Guid studentId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, studentId);
            return task != null ? _mapper.Map<TaskDTO>(task) : null;
        }

        public async Task<DashboardDTO> GetDashboardDataAsync(Guid studentId)
        {
            var todayTasks = await GetTodayTasksAsync(studentId);
            var upcomingTasks = await GetUpcomingTasksAsync(studentId);
            var overdueTasks = await GetOverdueTasksAsync(studentId);
            var totalTasks = await _taskRepository.GetTotalTasksCountAsync(studentId);
            var completedTasks = await _taskRepository.GetCompletedTasksCountAsync(studentId);
            var subjects = await _subjectRepository.GetByStudentIdAsync(studentId);

            var progressPercentage = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;

            return new DashboardDTO
            {
                TodayTasks = todayTasks.ToList(),
                UpcomingTasks = upcomingTasks.ToList(),
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                ProgressPercentage = Math.Round(progressPercentage, 1),
                HasNoTasks = totalTasks == 0
            };
        }

        public async Task<TaskDTO> CreateTaskAsync(TaskCreateDTO dto)
        {
            // Validate subject exists if provided
            if (dto.SubjectId.HasValue)
            {
                var subjectExists = await _subjectRepository.ExistsAsync(dto.SubjectId.Value, dto.StudentId);
                if (!subjectExists)
                {
                    throw new InvalidOperationException("Selected subject not found or access denied.");
                }
            }

            // Validate deadline is not in the past
            if (dto.Deadline < DateTime.Now.AddMinutes(-5)) // Allow 5 minutes tolerance
            {
                throw new InvalidOperationException("Deadline cannot be in the past.");
            }

            var task = _mapper.Map<Domain.Entities.Task>(dto);
            var createdTask = await _taskRepository.CreateAsync(task);
            return _mapper.Map<TaskDTO>(createdTask);
        }

        public async Task<TaskDTO> UpdateTaskAsync(TaskUpdateDTO dto)
        {
            // Check if task exists and belongs to student
            var existingTask = await _taskRepository.GetByIdAsync(dto.Id, dto.StudentId);
            if (existingTask == null)
            {
                throw new InvalidOperationException("Task not found or access denied.");
            }

            // Validate subject exists if provided
            if (dto.SubjectId.HasValue)
            {
                var subjectExists = await _subjectRepository.ExistsAsync(dto.SubjectId.Value, dto.StudentId);
                if (!subjectExists)
                {
                    throw new InvalidOperationException("Selected subject not found or access denied.");
                }
            }

            // Update the existing task
            existingTask.Title = dto.Title;
            existingTask.Description = dto.Description;
            existingTask.Deadline = dto.Deadline;
            existingTask.IsDone = dto.IsDone;
            existingTask.SubjectId = dto.SubjectId;

            var updatedTask = await _taskRepository.UpdateAsync(existingTask);
            return _mapper.Map<TaskDTO>(updatedTask);
        }

        public async Task<bool> DeleteTaskAsync(Guid taskId, Guid studentId)
        {
            return await _taskRepository.DeleteAsync(taskId, studentId);
        }

        public async Task<bool> ToggleTaskStatusAsync(Guid taskId, Guid studentId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, studentId);
            if (task == null)
            {
                return false;
            }

            task.IsDone = !task.IsDone;
            await _taskRepository.UpdateAsync(task);
            return true;
        }

        public async Task<bool> TaskExistsAsync(Guid taskId, Guid studentId)
        {
            return await _taskRepository.ExistsAsync(taskId, studentId);
        }

        public async Task<IEnumerable<TaskDTO>> GetTodayTasksAsync(Guid studentId)
        {
            var tasks = await _taskRepository.GetTodayTasksAsync(studentId);
            return _mapper.Map<IEnumerable<TaskDTO>>(tasks);
        }

        public async Task<IEnumerable<TaskDTO>> GetUpcomingTasksAsync(Guid studentId, int days = 7)
        {
            var tasks = await _taskRepository.GetUpcomingTasksAsync(studentId, days);
            return _mapper.Map<IEnumerable<TaskDTO>>(tasks);
        }

        public async Task<IEnumerable<TaskDTO>> GetOverdueTasksAsync(Guid studentId)
        {
            var tasks = await _taskRepository.GetOverdueTasksAsync(studentId);
            return _mapper.Map<IEnumerable<TaskDTO>>(tasks);
        }
    }
}