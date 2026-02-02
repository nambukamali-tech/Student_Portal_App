using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Students_Portal_App.Data;
using Students_Portal_App.Interfaces;
using Students_Portal_App.Models.Entities;
using Students_Portal_App.ViewModels;
using Students_Portal_App.DTOs;

public class StudentsCRUDService : IStudentServiceInterface 
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IAttendanceService _attendanceService;

    public StudentsCRUDService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment ,
        IAttendanceService attendanceService)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _attendanceService = attendanceService;
    }
    public async Task<Add_EditStudentVm?> AddStudentAsync(Add_EditStudentVm model, IFormFile? photo)
    {
        try
        {
            var student = new StudentsPortalInfos
            {
                RegisterNumber = model.RegisterNumber,
                StudentName = model.StudentName!,
                DepartmentId = model.DepartmentId!.Value
            };

            if (photo != null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "students");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(photo.FileName);
                var fileName = $"student_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await photo.CopyToAsync(stream);

                student.PhotoPath = "/uploads/students/" + fileName;
            }

            _context.StudentsPortalInfos.Add(student);
            await _context.SaveChangesAsync();

            model.StudentId = student.StudentId;
            model.PhotoPath = student.PhotoPath;

            return model;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in AddStudentAsync: " + ex.Message);
            return null;
        }
    }
    public async Task<Add_EditStudentVm?> GetStudentByIdAsync(int studentId)
    {
        var student = await _context.StudentsPortalInfos
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null) return null;

        return new Add_EditStudentVm
        {
            StudentId = student.StudentId,
            StudentName = student.StudentName,
            DepartmentId = student.DepartmentId,
            PhotoPath = student.PhotoPath,
            RegisterNumber = student.RegisterNumber
        };
    }

    public async Task<Add_EditStudentVm?> UpdateStudentAsync(Add_EditStudentVm model, IFormFile? photo)
    {
        try
        {
            var student = await _context.StudentsPortalInfos.FindAsync(model.StudentId);
            if (student == null) return null;

            student.StudentName = model.StudentName;
            student.DepartmentId = (int)model.DepartmentId;

            if (photo != null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "students");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(photo.FileName).ToLower();
                var fileName = $"student_{student.StudentId}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await photo.CopyToAsync(stream);

                student.PhotoPath = "/uploads/students/" + fileName;
            }

            await _context.SaveChangesAsync();

            model.PhotoPath = student.PhotoPath;
            return model;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in UpdateStudentAsync: " + ex.Message);
            return null;
        }
    }

    public async Task DeleteStudentAsync(int studentId)
    {
        var student = await _context.StudentsPortalInfos.FindAsync(studentId);
        if (student == null) return;

        _context.StudentsPortalInfos.Remove(student);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Add_EditStudentVm>> GetAllStudentsAsync()
    {
        var students = await _context.StudentsPortalInfos.ToListAsync();
        return students.Select(s => new Add_EditStudentVm
        {
            StudentId = s.StudentId,
            StudentName = s.StudentName,
            DepartmentId = s.DepartmentId,
            PhotoPath = s.PhotoPath
        }).ToList();
    }

    public async Task<List<Department>> GetDepartmentsAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<StudentFormViewmodel> GetStudentFormViewModelAsync()
    {
        var departments = await GetDepartmentsAsync();

        return new StudentFormViewmodel
        {
            Student = new Add_EditStudentVm(), 
            Departments = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName
            }).ToList()
        };
    }

    public async Task<StudentEditFormViewmodel> GetStudentEditFormViewModelAsync(int studentId)
    {
        var studentVm = await GetStudentByIdAsync(studentId);
        if (studentVm == null) return null;

        var departments = await GetDepartmentsAsync();

        return new StudentEditFormViewmodel
        {
            Student = studentVm,
            Departments = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName,
                Selected = d.DepartmentId == studentVm.DepartmentId
            }).ToList()
        };
    }
    public async Task<IndexViewModel> GetDashboardDataAsync()
    {
        var totalStudents = await _context.StudentsPortalInfos.CountAsync();
        var totalDepartments = await _context.Departments.CountAsync();

        return new IndexViewModel
        {
            TotalStudents = totalStudents,
            TotalDepartments = totalDepartments,
            StudentsAttendance = new List<StudentsAttendanceDto>(), 
            AttendanceSummary = new AttendanceResultDto(), 
            Departments = await _context.Departments.Select(d => d.DepartmentName).ToListAsync()
        };
    }


    public async Task<StaffDashboardViewModel> GetStaffDashboardAsync()
    {
        var today = DateTime.Today;

        // 1️⃣ Fetch students and departments
        var students = await _context.StudentsPortalInfos
            .Include(s => s.Department)
            .ToListAsync();

        var departmentsList = await _context.Departments.ToListAsync();

        // 2️⃣ Fetch attendances
        var todayAttendances = await _context.Attendances
            .Where(a => a.AttendanceDate == today)
            .ToListAsync();

        var monthAttendances = await _context.Attendances
            .Where(a => a.AttendanceDate.Month == today.Month && a.AttendanceDate.Year == today.Year)
            .ToListAsync();

        // 3️⃣ Map Today data
        var studentsToday = students.Select(s =>
        {
            var att = todayAttendances.FirstOrDefault(a => a.StudentId == s.StudentId);
            return new StudentsAttendanceDto
            {
                StudentId = s.StudentId,
                StudentName = s.StudentName ?? "Unknown",
                RegisterNumber = s.RegisterNumber ?? "Unknown",
                DepartmentName = s.Department?.DepartmentName ?? "Unknown",
                PhotoPath = s.PhotoPath,
                InTime = att?.InTime,
                OutTime = att?.OutTime,
                StudentStatus = att?.InTime != null ? "Active" : "Inactive"
            };
        }).ToList();

        // 4️⃣ Map Monthly data
        var studentsMonthly = students.Select(s =>
        {
            var attList = monthAttendances.Where(a => a.StudentId == s.StudentId).ToList();
            return new StudentsAttendanceDto
            {
                StudentId = s.StudentId,
                StudentName = s.StudentName ?? "Unknown",
                RegisterNumber = s.RegisterNumber ?? "Unknown",
                DepartmentName = s.Department?.DepartmentName ?? "Unknown",
                PhotoPath = s.PhotoPath,
                InTime = attList.FirstOrDefault()?.InTime,
                OutTime = attList.FirstOrDefault()?.OutTime,
                StudentStatus = attList.Any(a => a.InTime != null) ? "Active" : "Inactive"
            };
        }).ToList();

        var activeToday = studentsToday.Where(s => s.StudentStatus == "Active").ToList();
        var inactiveToday = studentsToday.Where(s => s.StudentStatus == "Inactive").ToList();

        var departments = departmentsList.Select(d => new DepartmentAttendanceDto
        {
            DepartmentName = d.DepartmentName,
            Count = studentsToday.Count(s => s.DepartmentName == d.DepartmentName),
            StudentNames = studentsToday
                .Where(s => s.DepartmentName == d.DepartmentName)
                .Select(s => s.StudentName!)
                .ToList()
        }).ToList();

        return new StaffDashboardViewModel
        {
            TotalStudents = await _context.StudentsPortalInfos.CountAsync(),
            StudentsAttendance = studentsToday,
            MonthlyStudentsAttendance = studentsMonthly,
            Departments = departments.Select(d => d.DepartmentName).ToList(),
            AttendanceSummary = new AttendanceResultDto
            {
                ActiveCount = activeToday.Count,
                InactiveCount = inactiveToday.Count,
                ActiveNames = activeToday.Select(s => s.StudentName!).ToList(),
                InactiveNames = inactiveToday.Select(s => s.StudentName!).ToList(),
                Departments = departments
            }
        };
    }


    public async Task MarkAttendanceAsync(int studentId, DateTime inTime, DateTime outTime)
        {
        var today = DateTime.Today;
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.AttendanceDate == today);

        if (attendance == null)
        {
            _context.Attendances.Add(new Attendance
            {
                StudentId = studentId,
                AttendanceDate = today,
                InTime = inTime,
                OutTime = outTime
            });
        }
        else
        {
            attendance.InTime = inTime;
            attendance.OutTime = outTime;
        }

        await _context.SaveChangesAsync();
    }
}
