using AutoMapper;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Domain.Entities;

namespace SmartPlanner.Application.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Task mappings
            CreateMap<Domain.Entities.Task, TaskDTO>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : null));

            CreateMap<TaskCreateDTO, Domain.Entities.Task>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Subject, opt => opt.Ignore());

            CreateMap<TaskUpdateDTO, Domain.Entities.Task>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Subject, opt => opt.Ignore());

            // Subject mappings
            CreateMap<Subject, SubjectDTO>()
                .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count))
                .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.Tasks.Count(t => t.IsDone)))
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks));

            CreateMap<Domain.Entities.Task, SubjectTaskSummaryDTO>();

            CreateMap<SubjectCreateDTO, Subject>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore());

            CreateMap<SubjectUpdateDTO, Subject>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore());
        }
    }
}