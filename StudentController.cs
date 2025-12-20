using Microsoft.AspNetCore.Mvc;
using Students_Portal_App.Models.Entities;
using Students_Portal_App.Services;
using Students_Portal_App.Interfaces;

namespace Students_Portal_App.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentInformationsServices _studentsInfoServices;

        public StudentController(IStudentInformationsServices studentsInfoServices)
        {
            _studentsInfoServices = studentsInfoServices;
        }
        //GET : Student
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<StudentsPortalInfos> studentsInformations = await _studentsInfoServices.GetStudentsPortalInfosAsync();
            return View(studentsInformations);
        }
        //Add Students GET:
        [HttpGet]
        public IActionResult AddStudent()
        { 
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentsPortalInfos students)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, show the same page again
                return View(students);
            }

            // Only save when model is valid
            await _studentsInfoServices.AddStudentsPortalInfosAsync(students);

            return RedirectToAction(nameof(Index));
        }

        //For Edit and Update operations
        [HttpGet]//Get method is important to fetch the data otherwise UI not appears
        public async Task<IActionResult> EditStudent()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentsPortalInfos studentsPortalInfos)
        {
            if (!ModelState.IsValid)
            {
                return View(studentsPortalInfos);
            }
            await _studentsInfoServices.UpdateStudentsPortalInfosAsync(studentsPortalInfos);
            return RedirectToAction(nameof(Index));
        }
        //Delete Student
        [HttpGet]
        public async Task<IActionResult> DeleteStudent(int studentId)
        {
            var student = await _studentsInfoServices.GetStudentByIdAsync(studentId);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int studentId)
        {
            await _studentsInfoServices.DeleteStudentsPortalInfosAsync(studentId);
            return RedirectToAction(nameof(Index));
        }

        //Get Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var model = await _studentsInfoServices.GetStudentsDashboardAsync();
            return View(model);
        }
        //Upload Photo
        [HttpGet]
        public IActionResult UploadPhoto(int studentId)
        {
            ViewBag.StudentId = studentId;
            return View();
        }

        [HttpPost]
       
        public async Task<IActionResult> UploadPhoto(int studentId, IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return Json(new { success = false, message = "Please select a photo" });

            var student = await _studentsInfoServices.GetStudentByIdAsync(studentId);
            if (student == null)
                return Json(new { success = false, message = "Student not found" });

            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(photo.FileName).ToLower();
            if (!allowedTypes.Contains(ext))
                return Json(new { success = false, message = "Only JPG or PNG allowed" });

            await _studentsInfoServices.UploadStudentPhotoAsync(studentId, photo);

            return Json(new { success = true, message = "Photo uploaded successfully" });
        }

        //View Photo
        [HttpGet]
        public async Task<IActionResult> ViewPhoto(int id)
        {
            var student = await _studentsInfoServices.GetStudentByIdAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }



    }
}
