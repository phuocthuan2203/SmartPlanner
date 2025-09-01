using System.ComponentModel.DataAnnotations;

namespace SmartPlanner.Application.DTOs
{
    public class SubjectCreateDTO
    {
        [Required(ErrorMessage = "Student ID is required")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Subject name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Subject name must be between 2 and 100 characters")]
        [Display(Name = "Subject Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    public class SubjectUpdateDTO
    {
        [Required(ErrorMessage = "Subject ID is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Student ID is required")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Subject name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Subject name must be between 2 and 100 characters")]
        [Display(Name = "Subject Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    public class SubjectDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public ICollection<SubjectTaskSummaryDTO> Tasks { get; set; } = new List<SubjectTaskSummaryDTO>();
    }

    public class SubjectTaskSummaryDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public DateTime Deadline { get; set; }
    }
}