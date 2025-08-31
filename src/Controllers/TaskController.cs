using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Application.Services.Interfaces;

namespace SmartPlanner.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ISubjectService _subjectService;

        public TaskController(ITaskService taskService, ISubjectService subjectService)
        {
            _taskService = taskService;
            _subjectService = subjectService;
        }

        public async Task<IActionResult> Index(TaskSearchDTO? search)
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var tasks = await _taskService.GetTasksByStudentAsync(studentId.Value, search);
            var subjects = await _subjectService.GetSubjectsByStudentAsync(studentId.Value);

            ViewBag.Subjects = new SelectList(subjects, "Id", "Name", search?.SubjectId);
            ViewBag.SearchModel = search ?? new TaskSearchDTO();

            return View(tasks);
        }

        public async Task<IActionResult> Create()
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            await PopulateSubjectsDropdown(studentId.Value);
            var model = new TaskCreateDTO { StudentId = studentId.Value };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskCreateDTO dto)
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            dto.StudentId = studentId.Value;

            if (!ModelState.IsValid)
            {
                await PopulateSubjectsDropdown(studentId.Value, dto.SubjectId);
                return View(dto);
            }

            try
            {
                await _taskService.CreateTaskAsync(dto);
                TempData["SuccessMessage"] = "Task created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateSubjectsDropdown(studentId.Value, dto.SubjectId);
                return View(dto);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the task. Please try again.");
                await PopulateSubjectsDropdown(studentId.Value, dto.SubjectId);
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var task = await _taskService.GetTaskByIdAsync(id, studentId.Value);
            if (task == null)
            {
                TempData["ErrorMessage"] = "Task not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new TaskUpdateDTO
            {
                Id = task.Id,
                StudentId = task.StudentId,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                IsDone = task.IsDone,
                SubjectId = task.SubjectId
            };

            await PopulateSubjectsDropdown(studentId.Value, task.SubjectId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaskUpdateDTO dto)
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            dto.StudentId = studentId.Value;

            if (!ModelState.IsValid)
            {
                await PopulateSubjectsDropdown(studentId.Value, dto.SubjectId);
                return View(dto);
            }

            try
            {
                await _taskService.UpdateTaskAsync(dto);
                TempData["SuccessMessage"] = "Task updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateSubjectsDropdown(studentId.Value, dto.SubjectId);
                return View(dto);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating the task. Please try again.");
                await PopulateSubjectsDropdown(studentId.Value, dto.SubjectId);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return Json(new { success = false, message = "Authentication required." });
            }

            try
            {
                var result = await _taskService.DeleteTaskAsync(id, studentId.Value);
                if (result)
                {
                    return Json(new { success = true, message = "Task deleted successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Task not found." });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while deleting the task." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return Json(new { success = false, message = "Authentication required." });
            }

            try
            {
                var result = await _taskService.ToggleTaskStatusAsync(id, studentId.Value);
                if (result)
                {
                    return Json(new { success = true, message = "Task status updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Task not found." });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating the task status." });
            }
        }

        private async Task PopulateSubjectsDropdown(Guid studentId, Guid? selectedSubjectId = null)
        {
            var subjects = await _subjectService.GetSubjectsByStudentAsync(studentId);
            ViewBag.Subjects = new SelectList(subjects, "Id", "Name", selectedSubjectId);
        }

        private Guid? GetStudentIdFromSession()
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            return Guid.TryParse(studentIdString, out var studentId) ? studentId : null;
        }
    }
}