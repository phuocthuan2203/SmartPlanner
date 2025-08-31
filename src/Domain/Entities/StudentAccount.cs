using System;
using System.Collections.Generic;

namespace SmartPlanner.Domain.Entities
{
    public class StudentAccount
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
