using SmartPlanner.Domain.Entities;

namespace SmartPlanner.Infrastructure.Repositories
{
    public interface ISubjectRepository
    {
        Task<IEnumerable<Subject>> GetByStudentIdAsync(Guid studentId);
        Task<Subject?> GetByIdAsync(Guid id, Guid studentId);
        Task<Subject?> GetByIdAsync(Guid id);
        Task<Subject> CreateAsync(Subject subject);
        Task<Subject> UpdateAsync(Subject subject);
        Task<bool> DeleteAsync(Guid id, Guid studentId);
        Task<bool> ExistsAsync(Guid id, Guid studentId);
        Task<bool> NameExistsAsync(string name, Guid studentId, Guid? excludeId = null);
    }
}