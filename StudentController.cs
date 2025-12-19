using Microsoft.AspNetCore.Mvc;
using Student_API.Models.Entities;
using Student_API.Interfaces;
using Student_API.Data;
using Student_API.Services;

namespace Student_API.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentsInfoServices _studentsInfoServices;
        
        public StudentController(IStudentsInfoServices studentsInfoServices)
        {
            _studentsInfoServices = studentsInfoServices;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<StudentsInformations> studentsInformations = await _studentsInfoServices.GetStudentsInformationsAsync();
            return View(studentsInformations);
        }
        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentsInformations students)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, show the same page again
                return View(students);
            }

            // Only save when model is valid
            await _studentsInfoServices.AddStudentsInformationsAsync(students);

            return RedirectToAction(nameof(Index));
        }


        //For Edit and Update operations
        [HttpGet]//Get method is important to fetch the data otherwise UI not appears
        public async Task<IActionResult> EditStudent()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentsInformations students)
        {
            if(!ModelState.IsValid)
            {
                return View(students);
            }
            await _studentsInfoServices.UpdateStudentsInformationsAsync(students);
            return RedirectToAction(nameof(Index));
        }

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
            await _studentsInfoServices.DeleteStudentsInformationsAsync(studentId);
            return RedirectToAction(nameof(Index));
        }
    }
}


    

