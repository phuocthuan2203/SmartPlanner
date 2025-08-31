using AutoMapper;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Domain.Entities;
using SmartPlanner.Infrastructure.Repositories;

namespace SmartPlanner.Application.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public SubjectService(ISubjectRepository subjectRepository, IMapper mapper)
        {
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubjectDTO>> GetSubjectsByStudentAsync(Guid studentId)
        {
            var subjects = await _subjectRepository.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<SubjectDTO>>(subjects);
        }

        public async Task<SubjectDTO?> GetSubjectByIdAsync(Guid subjectId, Guid studentId)
        {
            var subject = await _subjectRepository.GetByIdAsync(subjectId, studentId);
            return subject != null ? _mapper.Map<SubjectDTO>(subject) : null;
        }

        public async Task<SubjectDTO> CreateSubjectAsync(SubjectCreateDTO dto)
        {
            // Validate that subject name doesn't already exist for this student
            if (await _subjectRepository.NameExistsAsync(dto.Name, dto.StudentId))
            {
                throw new InvalidOperationException($"A subject with the name '{dto.Name}' already exists.");
            }

            var subject = _mapper.Map<Subject>(dto);
            var createdSubject = await _subjectRepository.CreateAsync(subject);
            return _mapper.Map<SubjectDTO>(createdSubject);
        }

        public async Task<SubjectDTO> UpdateSubjectAsync(SubjectUpdateDTO dto)
        {
            // Check if subject exists and belongs to student
            var existingSubject = await _subjectRepository.GetByIdAsync(dto.Id, dto.StudentId);
            if (existingSubject == null)
            {
                throw new InvalidOperationException("Subject not found or access denied.");
            }

            // Validate that subject name doesn't already exist for this student (excluding current subject)
            if (await _subjectRepository.NameExistsAsync(dto.Name, dto.StudentId, dto.Id))
            {
                throw new InvalidOperationException($"A subject with the name '{dto.Name}' already exists.");
            }

            // Update the existing subject
            existingSubject.Name = dto.Name;
            existingSubject.Description = dto.Description;

            var updatedSubject = await _subjectRepository.UpdateAsync(existingSubject);
            return _mapper.Map<SubjectDTO>(updatedSubject);
        }

        public async Task<bool> DeleteSubjectAsync(Guid subjectId, Guid studentId)
        {
            var subject = await _subjectRepository.GetByIdAsync(subjectId, studentId);
            if (subject == null)
            {
                return false;
            }

            // Check if subject has tasks
            if (subject.Tasks.Any())
            {
                throw new InvalidOperationException("Cannot delete subject that has associated tasks. Please delete or reassign the tasks first.");
            }

            return await _subjectRepository.DeleteAsync(subjectId, studentId);
        }

        public async Task<bool> SubjectExistsAsync(Guid subjectId, Guid studentId)
        {
            return await _subjectRepository.ExistsAsync(subjectId, studentId);
        }

        public async Task<bool> SubjectNameExistsAsync(string name, Guid studentId, Guid? excludeId = null)
        {
            return await _subjectRepository.NameExistsAsync(name, studentId, excludeId);
        }
    }
}