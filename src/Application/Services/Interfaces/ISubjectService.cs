using SmartPlanner.Application.DTOs;

namespace SmartPlanner.Application.Services.Interfaces
{
    public interface ISubjectService
    {
        Task<IEnumerable<SubjectDTO>> GetSubjectsByStudentAsync(Guid studentId);
        Task<SubjectDTO?> GetSubjectByIdAsync(Guid subjectId, Guid studentId);
        Task<SubjectDTO> CreateSubjectAsync(SubjectCreateDTO dto);
        Task<SubjectDTO> UpdateSubjectAsync(SubjectUpdateDTO dto);
        Task<bool> DeleteSubjectAsync(Guid subjectId, Guid studentId);
        Task<bool> SubjectExistsAsync(Guid subjectId, Guid studentId);
        Task<bool> SubjectNameExistsAsync(string name, Guid studentId, Guid? excludeId = null);
    }
}