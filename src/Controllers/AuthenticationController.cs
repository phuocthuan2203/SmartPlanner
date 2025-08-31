using Microsoft.AspNetCore.Mvc;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Application.Services.Interfaces;

namespace SmartPlanner.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authService;

        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _authService.LoginAsync(dto);
            if (result.Success)
            {
                // Store token in session or cookie for simplicity
                HttpContext.Session.SetString("AuthToken", result.Token);
                HttpContext.Session.SetString("StudentId", result.StudentId.ToString()!);
                HttpContext.Session.SetString("StudentName", result.StudentName!);
                
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.ErrorMessage = result.ErrorMessage;
            return View(dto);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(StudentRegisterDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _authService.RegisterAsync(dto);
            if (result.Success)
            {
                // Store token in session
                HttpContext.Session.SetString("AuthToken", result.Token);
                HttpContext.Session.SetString("StudentId", result.StudentId.ToString()!);
                HttpContext.Session.SetString("StudentName", result.StudentName!);
                
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.ErrorMessage = result.ErrorMessage;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                await _authService.LogoutAsync(token);
            }
            
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
