using Microsoft.AspNetCore.Mvc;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Application.Services.Interfaces;

namespace SmartPlanner.Controllers
{
    public class SubjectController : Controller
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        public async Task<IActionResult> Index()
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var subjects = await _subjectService.GetSubjectsByStudentAsync(studentId.Value);
            return View(subjects);
        }

        public IActionResult Create()
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var model = new SubjectCreateDTO { StudentId = studentId.Value };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectCreateDTO dto)
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            dto.StudentId = studentId.Value;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _subjectService.CreateSubjectAsync(dto);
                TempData["SuccessMessage"] = "Subject created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the subject. Please try again.");
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

            var subject = await _subjectService.GetSubjectByIdAsync(id, studentId.Value);
            if (subject == null)
            {
                TempData["ErrorMessage"] = "Subject not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new SubjectUpdateDTO
            {
                Id = subject.Id,
                StudentId = subject.StudentId,
                Name = subject.Name,
                Description = subject.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubjectUpdateDTO dto)
        {
            var studentId = GetStudentIdFromSession();
            if (studentId == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            dto.StudentId = studentId.Value;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _subjectService.UpdateSubjectAsync(dto);
                TempData["SuccessMessage"] = "Subject updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating the subject. Please try again.");
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
                var result = await _subjectService.DeleteSubjectAsync(id, studentId.Value);
                if (result)
                {
                    return Json(new { success = true, message = "Subject deleted successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Subject not found." });
                }
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while deleting the subject." });
            }
        }

        private Guid? GetStudentIdFromSession()
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            return Guid.TryParse(studentIdString, out var studentId) ? studentId : null;
        }
    }
}