using Microsoft.EntityFrameworkCore;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Infrastructure.Data;
using TaskStatus = SmartPlanner.Application.DTOs.TaskStatus;

namespace SmartPlanner.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly SmartPlannerDbContext _context;

        public TaskRepository(SmartPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetByStudentIdAsync(Guid studentId, TaskSearchDTO? search = null)
        {
            var query = _context.Tasks
                .Include(t => t.Subject)
                .Where(t => t.StudentId == studentId);

            if (search != null)
            {
                // Apply search filters
                if (!string.IsNullOrWhiteSpace(search.SearchTerm))
                {
                    var searchTerm = search.SearchTerm.ToLower();
                    query = query.Where(t => t.Title.ToLower().Contains(searchTerm) || 
                                           (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
                }

                if (search.SubjectId.HasValue)
                {
                    query = query.Where(t => t.SubjectId == search.SubjectId.Value);
                }

                if (search.Status.HasValue)
                {
                    switch (search.Status.Value)
                    {
                        case TaskStatus.Completed:
                            query = query.Where(t => t.IsDone);
                            break;
                        case TaskStatus.Pending:
                            query = query.Where(t => !t.IsDone && t.Deadline >= DateTime.Now);
                            break;
                        case TaskStatus.Overdue:
                            query = query.Where(t => !t.IsDone && t.Deadline < DateTime.Now);
                            break;
                    }
                }

                if (search.FromDate.HasValue)
                {
                    query = query.Where(t => t.Deadline >= search.FromDate.Value);
                }

                if (search.ToDate.HasValue)
                {
                    query = query.Where(t => t.Deadline <= search.ToDate.Value.AddDays(1));
                }

                // Apply sorting
                query = search.SortBy switch
                {
                    TaskSortBy.Title => search.SortOrder == SortOrder.Ascending 
                        ? query.OrderBy(t => t.Title) 
                        : query.OrderByDescending(t => t.Title),
                    TaskSortBy.CreatedAt => search.SortOrder == SortOrder.Ascending 
                        ? query.OrderBy(t => t.CreatedAt) 
                        : query.OrderByDescending(t => t.CreatedAt),
                    TaskSortBy.Subject => search.SortOrder == SortOrder.Ascending 
                        ? query.OrderBy(t => t.Subject != null ? t.Subject.Name : "") 
                        : query.OrderByDescending(t => t.Subject != null ? t.Subject.Name : ""),
                    _ => search.SortOrder == SortOrder.Ascending 
                        ? query.OrderBy(t => t.Deadline) 
                        : query.OrderByDescending(t => t.Deadline)
                };
            }
            else
            {
                query = query.OrderBy(t => t.Deadline);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetByStudentAndSubjectAsync(Guid studentId, Guid subjectId)
        {
            return await _context.Tasks
                .Include(t => t.Subject)
                .Where(t => t.StudentId == studentId && t.SubjectId == subjectId)
                .OrderBy(t => t.Deadline)
                .ToListAsync();
        }

        public async Task<Domain.Entities.Task?> GetByIdAsync(Guid id, Guid studentId)
        {
            return await _context.Tasks
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(t => t.Id == id && t.StudentId == studentId);
        }

        public async Task<Domain.Entities.Task?> GetByIdAsync(Guid id)
        {
            return await _context.Tasks
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Domain.Entities.Task> CreateAsync(Domain.Entities.Task task)
        {
            task.Id = Guid.NewGuid();
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<Domain.Entities.Task> UpdateAsync(Domain.Entities.Task task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid studentId)
        {
            var task = await GetByIdAsync(id, studentId);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id, Guid studentId)
        {
            return await _context.Tasks
                .AnyAsync(t => t.Id == id && t.StudentId == studentId);
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetTodayTasksAsync(Guid studentId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.Tasks
                .Include(t => t.Subject)
                .Where(t => t.StudentId == studentId && 
                           t.Deadline >= today && 
                           t.Deadline < tomorrow)
                .OrderBy(t => t.Deadline)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetUpcomingTasksAsync(Guid studentId, int days = 7)
        {
            var today = DateTime.Today;
            var futureDate = today.AddDays(days);

            return await _context.Tasks
                .Include(t => t.Subject)
                .Where(t => t.StudentId == studentId && 
                           !t.IsDone &&
                           t.Deadline > today && 
                           t.Deadline <= futureDate)
                .OrderBy(t => t.Deadline)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetOverdueTasksAsync(Guid studentId)
        {
            var now = DateTime.Now;

            return await _context.Tasks
                .Include(t => t.Subject)
                .Where(t => t.StudentId == studentId && 
                           !t.IsDone && 
                           t.Deadline < now)
                .OrderBy(t => t.Deadline)
                .ToListAsync();
        }

        public async Task<int> GetTotalTasksCountAsync(Guid studentId)
        {
            return await _context.Tasks
                .CountAsync(t => t.StudentId == studentId);
        }

        public async Task<int> GetCompletedTasksCountAsync(Guid studentId)
        {
            return await _context.Tasks
                .CountAsync(t => t.StudentId == studentId && t.IsDone);
        }
    }
}