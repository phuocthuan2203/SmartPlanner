using System.ComponentModel.DataAnnotations;

namespace SmartPlanner.Application.DTOs
{
    public class TaskCreateDTO
    {
        [Required(ErrorMessage = "Student ID is required")]
        public Guid StudentId { get; set; }

        [Display(Name = "Subject")]
        public Guid? SubjectId { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters")]
        [Display(Name = "Task Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Deadline is required")]
        [Display(Name = "Deadline")]
        [DataType(DataType.DateTime)]
        public DateTime Deadline { get; set; } = DateTime.Today.AddDays(1);
    }

    public class TaskUpdateDTO
    {
        [Required(ErrorMessage = "Task ID is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Student ID is required")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters")]
        [Display(Name = "Task Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Deadline is required")]
        [Display(Name = "Deadline")]
        [DataType(DataType.DateTime)]
        public DateTime Deadline { get; set; }

        [Display(Name = "Completed")]
        public bool IsDone { get; set; }

        [Display(Name = "Subject")]
        public Guid? SubjectId { get; set; }
    }

    public class TaskDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsDone { get; set; }
        public Guid? SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsOverdue => !IsDone && Deadline < DateTime.Now;
        public string StatusText => IsDone ? "Completed" : (IsOverdue ? "Overdue" : "Pending");
    }

    public class TaskSearchDTO
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Subject")]
        public Guid? SubjectId { get; set; }

        [Display(Name = "Status")]
        public TaskStatus? Status { get; set; }

        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Sort By")]
        public TaskSortBy SortBy { get; set; } = TaskSortBy.Deadline;

        [Display(Name = "Sort Order")]
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
    }

    public class DashboardDTO
    {
        public List<TaskDTO> TodayTasks { get; set; } = new(); // Tasks due today, is_done = false
        public List<TaskDTO> UpcomingTasks { get; set; } = new(); // Tasks due after today, is_done = false
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public double ProgressPercentage { get; set; } // % tasks completed
        public bool HasNoTasks { get; set; } // For empty state handling
    }

    public enum TaskStatus
    {
        All = 0,
        Pending = 1,
        Completed = 2,
        Overdue = 3
    }

    public enum TaskSortBy
    {
        Title = 0,
        Deadline = 1,
        CreatedAt = 2,
        Subject = 3
    }

    public enum SortOrder
    {
        Ascending = 0,
        Descending = 1
    }
}
