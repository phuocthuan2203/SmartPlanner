using System;
using System.Collections.Generic;

namespace SmartPlanner.Domain.Entities
{
    public class Subject
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual StudentAccount Student { get; set; } = null!;
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}
