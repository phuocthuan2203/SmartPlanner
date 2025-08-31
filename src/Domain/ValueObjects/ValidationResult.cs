using System.Collections.Generic;

namespace SmartPlanner.Domain.ValueObjects
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        
        public ValidationResult() { }
        
        public ValidationResult(bool isValid, List<string>? errors = null)
        {
            IsValid = isValid;
            Errors = errors ?? new List<string>();
        }
        
        public static ValidationResult Success() => new(true);
        public static ValidationResult Failure(List<string> errors) => new(false, errors);
        public static ValidationResult Failure(string error) => new(false, new List<string> { error });
    }
}
