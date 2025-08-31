using System;

namespace SmartPlanner.Domain.Entities
{
    public class Task
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid? SubjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsDone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual StudentAccount Student { get; set; } = null!;
        public virtual Subject? Subject { get; set; }
    }
}
