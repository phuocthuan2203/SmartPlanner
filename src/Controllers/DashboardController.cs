using Microsoft.AspNetCore.Mvc;
using SmartPlanner.Application.Services.Interfaces;

namespace SmartPlanner.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            // Check session authentication
            var studentIdString = HttpContext.Session.GetString("StudentId");
            var studentName = HttpContext.Session.GetString("StudentName");
            
            if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
            {
                return RedirectToAction("Login", "Authentication");
            }
            
            try
            {
                // Get dashboard data using new dashboard service
                var dashboardData = await _dashboardService.BuildDashboardAsync(studentId);
                
                // Pass user information to view
                ViewBag.StudentName = studentName;
                ViewBag.StudentId = studentIdString;
                
                return View(dashboardData);
            }
            catch (Exception)
            {
                // If there's an error loading dashboard data, show empty dashboard
                ViewBag.StudentName = studentName;
                ViewBag.StudentId = studentIdString;
                ViewBag.ErrorMessage = "Unable to load dashboard data. Please try again.";
                
                return View(new SmartPlanner.Application.DTOs.DashboardDTO());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkTaskDone(Guid taskId)
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
            {
                return Json(new { success = false, message = "Authentication required" });
            }

            try
            {
                var success = await _dashboardService.MarkTaskDoneFromDashboardAsync(taskId, studentId);
                if (success)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Task not found or access denied" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating the task" });
            }
        }
    }
}