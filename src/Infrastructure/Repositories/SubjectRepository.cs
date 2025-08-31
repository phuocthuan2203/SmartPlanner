using Microsoft.EntityFrameworkCore;
using SmartPlanner.Domain.Entities;
using SmartPlanner.Infrastructure.Data;

namespace SmartPlanner.Infrastructure.Repositories
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly SmartPlannerDbContext _context;

        public SubjectRepository(SmartPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subject>> GetByStudentIdAsync(Guid studentId)
        {
            return await _context.Subjects
                .Include(s => s.Tasks)
                .Where(s => s.StudentId == studentId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Subject?> GetByIdAsync(Guid id, Guid studentId)
        {
            return await _context.Subjects
                .Include(s => s.Tasks)
                .FirstOrDefaultAsync(s => s.Id == id && s.StudentId == studentId);
        }

        public async Task<Subject?> GetByIdAsync(Guid id)
        {
            return await _context.Subjects
                .Include(s => s.Tasks)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Subject> CreateAsync(Subject subject)
        {
            subject.Id = Guid.NewGuid();
            subject.CreatedAt = DateTime.UtcNow;
            subject.UpdatedAt = DateTime.UtcNow;

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task<Subject> UpdateAsync(Subject subject)
        {
            subject.UpdatedAt = DateTime.UtcNow;
            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid studentId)
        {
            var subject = await GetByIdAsync(id, studentId);
            if (subject == null) return false;

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id, Guid studentId)
        {
            return await _context.Subjects
                .AnyAsync(s => s.Id == id && s.StudentId == studentId);
        }

        public async Task<bool> NameExistsAsync(string name, Guid studentId, Guid? excludeId = null)
        {
            var query = _context.Subjects
                .Where(s => s.StudentId == studentId && s.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}