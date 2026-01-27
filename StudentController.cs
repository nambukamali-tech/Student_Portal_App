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
        private readonly ApplicationDbContext _context; //Remove

        //_context is a private field of the class
        private readonly IAttendanceService _attendanceService;
        //dashboard service
        private readonly IDashboardService _dashboardService;
        //student service
        private readonly IStudentServiceInterface _studentService;
        

        public StudentController(IStudentInformationsServices studentsInfoServices,
            IStudentServiceInterface studentService,
            IAttendanceService attendanceService, IDashboardService dashboardService,
            ApplicationDbContext context)

        {
            _studentsInfoServices = studentsInfoServices;
            _studentsPaperService = studentsInfoServices;
            _context = context; //Dbcontext instance is injected into the controller via constructor
            _attendanceService = attendanceService; //need to clear the error here
            _dashboardService = dashboardService;
            _studentService = studentService;
            
        }
      
       //Controller for index page only handles the request and response
        [HttpGet]
        public async Task<IActionResult> Index()
        {         
            var attendanceSummary = await _attendanceService.GetIndexAsync();
            return View(attendanceSummary);       
        }

        //For Mark Attendance Get Method for handling the request and response
        [HttpPost]
        public async Task<IActionResult> MarkAttendance(int StudentId , DateTime? InTime, DateTime? OutTime)
        {
           var result = await _attendanceService.MarkAttendanceAsync(StudentId, InTime, OutTime);
            return Json(result);
        }

        // Add Student Get Method
        [HttpGet]
        public async Task<IActionResult> AddStudent()
        {
            var departments = await _studentService.GetDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");
            return View();
        }

        // Add Student Post Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(Add_EditStudentVm model, IFormFile? Photo)
        {
            if (!ModelState.IsValid)
            {
                // Get departments for dropdown
                var departments = await _studentService.GetDepartmentsAsync();
                ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", model.DepartmentId);

                return View(model);
            }

            // Call service to add student (handles entity mapping & photo upload)
            var addedStudent = await _studentService.AddStudentAsync(model, Photo);
            return RedirectToAction(nameof(Index));
        }

        //Edit Student Controller
        // GET: Edit Student
        [HttpGet]
        public async Task<IActionResult> EditStudent(int studentId)
        {
            // Fetch student from service
            var student = await _studentService.GetStudentByIdAsync(studentId);
            if (student == null) return NotFound();

            // Pass student data to ViewModel
            var model = new Add_EditStudentVm
            {
                StudentId = student.StudentId,
                RegisterNumber = student.RegisterNumber,  
                StudentName = student.StudentName,
                DepartmentId = student.DepartmentId,
                PhotoPath = student.PhotoPath
            };

            // Get department list for dropdown
            var departments = await _studentService.GetDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", model.DepartmentId);

            return View(model);
        }

        //Method handle the Request and Response of Edit Student Infos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(Add_EditStudentVm model, IFormFile? photo)
        {
            if (!ModelState.IsValid)
            {
                var departments = await _studentService.GetDepartmentsAsync();
                ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", model.DepartmentId);
                return View(model);
            }

            var updatedStudent = await _studentService.UpdateStudentAsync(model, photo);
            if (updatedStudent == null)
                return NotFound();
            return RedirectToAction(nameof(Index));
        }

        //Delete Student
        [HttpGet]
        public async Task<IActionResult> DeleteStudent(int studentId)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int studentId)
        {
            await _studentService.DeleteStudentAsync(studentId);
            return RedirectToAction(nameof(Index));
        }

        //CRUD Operations Finished

        //------- Papers Function -----

        //All for Add Papers to Departments
        //without this method list of departments not shown
        private void LoadDepartments() //For dropdown of datas for department names
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

        //Add paper post method
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
                //Concept here is , if i add TempDate it must be added to Razor View 
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

        //Get Department Papers View papers
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

 //.... student papers details .... finished

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

        //Student Dashboard controller
        [HttpGet]
        public async Task<IActionResult> Dashboard(string regno)
        {
            StudentDashboardVm? dashboardVm = null;

            if (!string.IsNullOrWhiteSpace(regno))
            {
                dashboardVm = await _dashboardService.GetStudentDashboardAsync(regno);
            }

            // If dashboardVm is null, create an empty model to prevent null refs in view
            if (dashboardVm == null)
            {
                dashboardVm = new StudentDashboardVm
                {
                    Student = new StudentsPortalInfos(), // Empty student
                    Papers = new List<DepartmentPapers>(),
                    Scholarships = new List<StudentScholarship>(),
                    MonthlyAttendance = new List<Attendance>()
                };
            }

            return View(dashboardVm);
        }
    }
}




