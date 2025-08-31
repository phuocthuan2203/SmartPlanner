namespace SmartPlanner.Application.Services.Interfaces
{
    public interface ISecurityService
    {
        string HashPassword(string plainPassword);
        bool VerifyPassword(string plainPassword, string passwordHash);
        string GenerateAuthToken(Guid studentId, string email, string fullName);
    }
}
