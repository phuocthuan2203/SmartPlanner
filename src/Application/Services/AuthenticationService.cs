using Microsoft.EntityFrameworkCore;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Domain.Entities;
using SmartPlanner.Domain.ValueObjects;
using SmartPlanner.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace SmartPlanner.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly SmartPlannerDbContext _context;
        private readonly ISecurityService _securityService;

        public AuthenticationService(SmartPlannerDbContext context, ISecurityService securityService)
        {
            _context = context;
            _securityService = securityService;
        }

        public async Task<AuthResponse> RegisterAsync(StudentRegisterDTO dto)
        {
            // Validation
            var validation = ValidateRegistration(dto);
            if (!validation.IsValid)
            {
                return new AuthResponse
                {
                    Success = false,
                    ErrorMessage = string.Join(", ", validation.Errors)
                };
            }

            // Check if email exists
            var existingUser = await _context.StudentAccounts.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (existingUser != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    ErrorMessage = "Account with this email already exists."
                };
            }

            // Create new student account
            var student = new StudentAccount
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = _securityService.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.StudentAccounts.Add(student);
            await _context.SaveChangesAsync();

            // Generate token
            var token = _securityService.GenerateAuthToken(student.Id, student.Email, student.FullName);

            return new AuthResponse
            {
                Success = true,
                Token = token,
                StudentId = student.Id,
                StudentName = student.FullName
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginDTO dto)
        {
            var student = await _context.StudentAccounts.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (student == null || !_securityService.VerifyPassword(dto.Password, student.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password."
                };
            }

            var token = _securityService.GenerateAuthToken(student.Id, student.Email, student.FullName);

            return new AuthResponse
            {
                Success = true,
                Token = token,
                StudentId = student.Id,
                StudentName = student.FullName
            };
        }

        public async Task LogoutAsync(string token)
        {
            // For JWT tokens, logout is typically handled client-side by discarding the token
            // In a more sophisticated system, you might maintain a blacklist of tokens
            await Task.CompletedTask;
        }

        private ValidationResult ValidateRegistration(StudentRegisterDTO dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Email))
                errors.Add("Email is required.");
            else if (!IsValidEmail(dto.Email))
                errors.Add("Email format is invalid.");

            if (string.IsNullOrWhiteSpace(dto.FullName))
                errors.Add("Full name is required.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                errors.Add("Password is required.");
            else if (dto.Password.Length < 6)
                errors.Add("Password must be at least 6 characters long.");

            if (dto.Password != dto.ConfirmPassword)
                errors.Add("Passwords do not match.");

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
