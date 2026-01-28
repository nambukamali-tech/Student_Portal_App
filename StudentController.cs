using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Students_Portal_App.Interfaces;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Controllers
{
    public class StudentController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IStudentServiceInterface _studentService;
           
        public StudentController(IStudentServiceInterface studentService,
            IAttendanceService attendanceService)
        {       
            _attendanceService = attendanceService;           
            _studentService = studentService;            
        }
      
        [HttpGet]
        public async Task<IActionResult> Index()
        {         
            var attendanceSummary = await _attendanceService.GetIndexAsync();
            return View(attendanceSummary);       
        }

        [HttpPost]
        public async Task<IActionResult> MarkAttendance(int StudentId , DateTime? InTime, DateTime? OutTime)
        {
           var result = await _attendanceService.MarkAttendanceAsync(StudentId, InTime, OutTime);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> AddStudent()
        {
            var model = await _studentService.GetStudentFormViewModelAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(StudentFormViewmodel model, IFormFile? Photo)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _studentService.AddStudentAsync(model.Student, Photo);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int studentId)
        {
            var model = await _studentService.GetStudentEditFormViewModelAsync(studentId);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentEditFormViewmodel model, IFormFile? photo)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _studentService.UpdateStudentAsync(model.Student, photo);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteStudent(int studentId)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId);
            if (student == null) return NotFound();
            return View(student); 
        }

        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentConfirmed(int studentId)
        {
            await _studentService.DeleteStudentAsync(studentId);
            TempData["SuccessMessage"] = "Student deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

    }
}




