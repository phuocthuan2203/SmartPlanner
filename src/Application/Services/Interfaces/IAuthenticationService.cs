using SmartPlanner.Application.DTOs;

namespace SmartPlanner.Application.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResponse> RegisterAsync(StudentRegisterDTO dto);
        Task<AuthResponse> LoginAsync(LoginDTO dto);
        Task LogoutAsync(string token);
    }
}
