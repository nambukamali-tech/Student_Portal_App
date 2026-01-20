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
        private readonly IStudentInformationsServices _studentsInfoServices;//take method from interface name ie) IStudentInformationsServices
      //_studentsInfoServices  is a private field of the class
        private readonly IStudentInformationsServices _studentsPaperService;
        //_studentsPaperService is a private field of the class
        private readonly ApplicationDbContext _context;
        //_context is a private field of the class

        public StudentController(IStudentInformationsServices studentsInfoServices, ApplicationDbContext context)
        {
            _studentsInfoServices = studentsInfoServices;
            _studentsPaperService = studentsInfoServices;
            _context = context;//Dbcontext instance is injected into the controller via constructor
            
        }
        //Controller Business logic for Attendance management
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allStudents = await _studentsInfoServices.GetStudentsPortalInfosAsync();

            ViewBag.TotalStudents = allStudents.ToList().Count;

            DateTime today = DateTime.Today;

            // Active students today (attendance-based)
            var activeTodayIds = await _context.Attendances
                .Where(a => a.AttendanceDate == today && a.InTime.HasValue)
                .Select(a => a.StudentId)
                .Distinct()
                .ToListAsync();

            ViewBag.ActiveTodayIds = activeTodayIds;

            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = departments.Select(d => new
            {
                d.DepartmentId,
                d.DepartmentName,
                StudentCount = allStudents.Count(s => s.DepartmentId == d.DepartmentId)
            }).ToList();

            return View(allStudents);
        }

        //For mark attendance      
        [HttpPost]
        public async Task<IActionResult> MarkAttendance(int studentId, DateTime? inTime, DateTime? outTime)
        {
            var today = DateTime.Today;

            //Add and update attendance
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.AttendanceDate == today);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    StudentId = studentId,
                    AttendanceDate = today,
                    InTime = inTime,
                    OutTime = outTime,
                    Status = inTime.HasValue ? "Active" : "Inactive"
                };
                _context.Attendances.Add(attendance);
            }
            else
            {
                attendance.InTime = inTime;
                attendance.OutTime = outTime;
                attendance.Status = inTime.HasValue ? "Active" : "Inactive";
                _context.Attendances.Update(attendance);
            }

            await _context.SaveChangesAsync();

            // Get all the students
            var allStudents = await _context.StudentsPortalInfos
                .Include(s => s.Department)
                .ToListAsync();

            // Get active students today
            var activeTodayIds = await _context.Attendances
                .Where(a => a.AttendanceDate == today && a.InTime.HasValue)
                .Select(a => a.StudentId)
                .ToListAsync();

            var activeToday = allStudents
                .Where(s => activeTodayIds.Contains(s.StudentId))
                .ToList();

            var inactiveToday = allStudents
                .Where(s => !activeTodayIds.Contains(s.StudentId))
                .ToList();

            //for department wise active students
            var department = activeToday
                .Where(s => s.Department != null)
                .GroupBy(s => s.Department.DepartmentName)
                .Select(g => new
                {
                    Department = g.Key,
                    Count = g.Count(),
                    Students = g.Select(x => x.StudentName).ToList()
                })
                .ToList();

            // To update chart dynamically 
            return Json(new
            {
                activeCount = activeToday.Count,
                inactiveCount = inactiveToday.Count,
                activeNames = activeToday.Select(x => x.StudentName).ToList(),
                inactiveNames = inactiveToday.Select(x => x.StudentName).ToList(),
                activeIds = activeToday.Select(x => x.StudentId).Distinct().ToList(),
                department
            });
        }

        // GET: Add Student
        [HttpGet]
        public async Task<IActionResult> AddStudent()
        {
            var departments = await _context.Departments.ToListAsync();

            ViewBag.Departments = new SelectList(
                departments,
                "DepartmentId",
                "DepartmentName"
            );

            return View();
        }

        // POST: Add Student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentsPortalInfos model, IFormFile? Photo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(
                    await _context.Departments.ToListAsync(),
                    "DepartmentId",
                    "DepartmentName",
                    model.DepartmentId
                );

                return View(model);
            }


            // Handle photo upload
            if (Photo != null && Photo.Length > 0)
            {
                if (!Photo.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("Photo", "Only image files are allowed.");
                    ViewBag.Departments = new SelectList(
                        await _context.Departments.ToListAsync(),
                        "DepartmentId",
                        "DepartmentName"
                    );
                    return View(model);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/students");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Photo.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await Photo.CopyToAsync(stream);

                model.PhotoPath = $"/uploads/students/{fileName}";
            }

             await _studentsInfoServices.AddStudentsPortalInfosAsync(model);
            return RedirectToAction("Index");
        }

        //For Update Time controller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTime(UpdateTimedtos model)//Request receive and response using IActionResult
        {
            if (model.StudentId <= 0)
                return BadRequest("Invalid student ID");

            var student = await _studentsInfoServices.GetStudentByIdAsync(model.StudentId);
            if (student == null)
                return NotFound("Student not found");

            student.InTime = model.InTime;
            student.OutTime = model.OutTime;

            await _studentsInfoServices.UpdateStudentsPortalInfosAsync(student);

            return Ok();//200 response
        }


        //For Edit and Update operations of students details
        [HttpGet]
        public async Task<IActionResult> EditStudent(int studentId)
        {
            var student = await _studentsInfoServices.GetStudentByIdAsync(studentId);

            if (student == null)
                return NotFound();

            //It automatically gives the department details in dropdown of view page
            //When user clicks select department itshows the names of departments
            ViewBag.Departments = new SelectList(
                await _context.Departments.ToListAsync(),
                "DepartmentId",
                "DepartmentName",
                student.DepartmentId
            );

            return View(student);
        }

        //Method handle the Request and Response of Edit Student Infos

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(StudentsPortalInfos model, IFormFile? Photo)
        {
            if (!ModelState.IsValid)
            {
                // Populate the departments again so dropdown works on validation error
                ViewBag.Departments = new SelectList(
                    await _context.Departments.ToListAsync(),
                    "DepartmentId",
                    "DepartmentName",
                    model.DepartmentId
                );

                return View(model);
            }

            // Handle photo upload
            if (Photo != null && Photo.Length > 0)
            {
                if (!Photo.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("Photo", "Only image files are allowed.");
                    ViewBag.Departments = new SelectList(
                        await _context.Departments.ToListAsync(),
                        "DepartmentId",
                        "DepartmentName",
                        model.DepartmentId
                    );
                    return View(model);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/students");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Photo.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await Photo.CopyToAsync(stream);

                model.PhotoPath = $"/uploads/students/{fileName}";
            }
            else
            {
                // Keep existing photo if no new photo uploaded
                var existing = await _studentsInfoServices.GetStudentByIdAsync(model.StudentId);
                model.PhotoPath = existing?.PhotoPath;
            }

            // Update student in database
            await _studentsInfoServices.UpdateStudentsPortalInfosAsync(model);

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




