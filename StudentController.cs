using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Students_Portal_App.Data;
using Students_Portal_App.DTOs;
using Students_Portal_App.Interfaces;
using Students_Portal_App.Models.Entities;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentInformationsServices _studentsInfoServices;
        private readonly IStudentInformationsServices _studentsPaperService;
        private readonly ApplicationDbContext _context;


        public StudentController(IStudentInformationsServices studentsInfoServices, ApplicationDbContext context)
        {
            _studentsInfoServices = studentsInfoServices;
            _studentsPaperService = studentsInfoServices;
            _context = context;
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
        public async Task<IActionResult> AddStudent(StudentsPortalInfos model, IFormFile? Photo)
        {
            // 1️⃣ Validate Model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2️⃣ Generate StudentId manually if needed
            int lastId = await _context.StudentsPortalInfos
                .OrderByDescending(s => s.StudentId)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync();

            model.StudentId = lastId == 0 ? 1001 : lastId + 1;

            // 3️⃣ Handle photo upload
            if (Photo != null && Photo.Length > 0)
            {
                // Set a folder path, e.g., wwwroot/uploads/students
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/students");

                // Create folder if not exists
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Unique file name
                var fileName = $"{Guid.NewGuid()}_{Photo.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Photo.CopyToAsync(stream);
                }

                // Store relative path in DB
                model.PhotoPath = $"/uploads/students/{fileName}";
            }

            // 4️⃣ Save using service layer
            await _studentsInfoServices.AddStudentsPortalInfosAsync(model);

            // 5️⃣ Redirect to index
            return RedirectToAction("Index");
        }

        //For Edit and Update operations
        [HttpGet]
        public async Task<IActionResult> EditStudent(int studentId)
        {
            var student = await _studentsInfoServices.GetStudentByIdAsync(studentId);

            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        //Delete Post method

        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int studentId)
        {
            await _studentsInfoServices.DeleteStudentsPortalInfosAsync(studentId);
            return RedirectToAction(nameof(Index));
        }

        //Get Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()//Get Dashboard
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

        //Upload photo Post
        [HttpPost]
        public async Task<IActionResult> UploadPhoto([FromForm] int studentId, [FromForm] IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return Json(new { success = false, message = "Please select a photo" });

            var student = await _studentsInfoServices.GetStudentByIdAsync(studentId);
            if (student == null)
                return Json(new { success = false, message = "Student not found" });

            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".webp" };//supports only four formats of photo
            var ext = Path.GetExtension(photo.FileName).ToLower();//convert to lowercase to avoid case sensitivity issues
            if (!allowedTypes.Contains(ext))
                return Json(new { success = false, message = "Only JPG or PNG or Webp  allowed" });

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
        //All for Add Papers to Departments
        private void LoadDepartments()
        {
            //Take the Department name and Id from Db mysql it already stored the values for it
            // Load departments from DB to ViewBag for dropdown
            ViewBag.Departments = new SelectList(
                _context.Departments.ToList(),  // fetch all departments from the mysql db
                "DepartmentId",                 // value submitted
                "DepartmentName"                // text shown in dropdown
            );
        }

        //Important to show the Addpaper page
        [HttpGet]
        public IActionResult AddPaper()
        {
            LoadDepartments();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPaper(AddPaperViewModel model)
        {
            // logic starts
            if (!ModelState.IsValid)
            {
                LoadDepartments();
                return View(model);
            }

            //  safety check 
            bool departmentExists = await _context.Departments
                .AnyAsync(d => d.DepartmentId == model.DepartmentId);

            if (!departmentExists)
            {
                ModelState.AddModelError("", "Invalid department selected");
                LoadDepartments();
                return View(model);
            }
            //Fields must match to the Papers ViewModel

            var paper = new DepartmentPapers
            {
                PaperCode = model.PaperCode!,
                PaperTitle = model.PaperTitle!,
                PaperDescription = model.PaperDescription,
                CourseName = model.CourseName!,
                DepartmentId = model.DepartmentId
            };

            try
            {
                await _studentsPaperService.AddPaperAsync(paper);
                //show success message 
                //Concept here is if i add TempDate it must be added to Razor View 
                TempData["SuccessMessage"] = "Papers added to Department Successfully";
                return RedirectToAction("AddPaper");
            }
            catch
            {
                TempData["ErrorMessage"] = "Papers Not Added Successfully !";
                LoadDepartments();
                return View(model);
            }
        }

        //Get Department Papers 
        [HttpGet]
        public async Task<IActionResult> GetDepartmentPapersAsync()
        {
            var papers = await _studentsPaperService.GetDepartmentPapersAsync();
            return View(papers);
        }

        //Add papers works correctly but the method inside the add papers for view papers not works properly
       // GET: View paper is real problem
        [HttpGet]
        public async Task<IActionResult> ViewPaper()
        {
            var papers = await _studentsPaperService.GetStudentsPapersAsync();
            return View(papers);
        }


        [HttpGet]
        public IActionResult SearchStudent()
        {
            return View("SearchDepartmentPapers", model: new SearchDepartmentPapersViewModel());

        }
        //Join two tables studentsportalinfos and student papers
        // JOIN PAGE
        [HttpGet]
        public async Task<IActionResult> StudentPaperDetails(int page = 1)
        {
            int pageSize = 10;

            var data = await _studentsInfoServices
                .GetStudentsWithPapersAsync(page, pageSize);

            var total = await _studentsInfoServices
                .GetStudentsWithPaperCountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View(data);
        }

        // Show the form to add scholarship

        [HttpGet]
        public async Task<IActionResult> ScholarshipsList()
        {
            var scholarships = await _studentsInfoServices.GetAllScholarshipsAsync();
            return View(scholarships);
        }
        // SHOW Add Scholarship Form
        [HttpGet]
        public IActionResult AddScholarship()
        {
            return View();
        }

        //Add scholarship 
        // Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddScholarship(AddScholarshipViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _studentsInfoServices.AddScholarshipUsingSP(model);
                TempData["Success"] = "Scholarship added successfully!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Dashboard");
        }

    }
}




