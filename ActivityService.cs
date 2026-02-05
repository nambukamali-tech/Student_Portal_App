using Microsoft.EntityFrameworkCore;
using Students_Portal_App.Data;
using Students_Portal_App.DTOs;
using Students_Portal_App.Interfaces;
using Students_Portal_App.Models.Entities;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Services
{
    public class ActivityService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDbAccessorService _dbAccessorService;
        public ActivityService(ApplicationDbContext context, IDbAccessorService dbAccessorService)
        {
            _context = context;
            _dbAccessorService = dbAccessorService;
        }
        public async Task<AttendanceResultDto> MarkAttendanceAsync(int studentId, DateTime? inTime, DateTime? outTime)
        {
            var today = DateTime.Today;
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
                    Status = inTime.HasValue ? "Present" : "Absent"
                };

                _context.Attendances.Add(attendance);
            }
            else
            {
                if (inTime != null)
                    attendance.InTime = inTime;

                if (outTime != null)
                    attendance.OutTime = outTime;

                attendance.Status = attendance.InTime.HasValue ? "Present" : "Absent";
            }

            await _context.SaveChangesAsync();

            int totalStudents = await _context.StudentsPortalInfos.CountAsync();

            var activeIds = await _context.Attendances
                .Where(a => a.AttendanceDate == today && a.InTime != null)
                .Select(a => a.StudentId)
                .Distinct()
                .ToListAsync();

            int activeCount = activeIds.Count;
            int inactiveCount = totalStudents - activeCount;
            return new AttendanceResultDto
            {
                ActiveCount = activeCount,
                InactiveCount = inactiveCount,
                ActiveIds = activeIds
            };
        }

        //Index method
        public async Task<IndexViewModel> GetIndexAsync()
        {
            var today = DateTime.Today;
            var month = today.Month;
            var year = today.Year;

            // Total students
            int totalStudents = await _dbAccessorService.GetTotalstudentCount();

            var studentsToday = await _context.StudentsPortalInfos
                .Include(s => s.Department)
                .Include(s => s.Attendances)
                .Select(s => new StudentsAttendanceDto
                {
                    StudentId = s.StudentId,
                    PhotoPath = s.PhotoPath,
                    StudentName = s.StudentName!,
                    DepartmentName = s.Department != null ? s.Department.DepartmentName : "",
                    RegisterNumber = s.RegisterNumber!,
                    StudentStatus = s.Attendances.Any(a => a.AttendanceDate == today && a.InTime.HasValue) ? "Active" : "Inactive",
                    InTime = s.Attendances.Where(a => a.AttendanceDate == today).Select(a => a.InTime).FirstOrDefault(),
                    OutTime = s.Attendances.Where(a => a.AttendanceDate == today).Select(a => a.OutTime).FirstOrDefault()
                })
                .ToListAsync();

            var studentsMonthly = await _context.StudentsPortalInfos
                .Include(s => s.Department)
                .Include(s => s.Attendances)
                .Select(s => new StudentsAttendanceDto
                {
                    StudentId = s.StudentId,
                    PhotoPath = s.PhotoPath,
                    StudentName = s.StudentName!,
                    RegisterNumber = s.RegisterNumber!,
                    DepartmentName = s.Department != null ? s.Department.DepartmentName : "",
                    StudentStatus = s.Attendances.Any(a => a.AttendanceDate.Month == month && a.AttendanceDate.Year == year && a.InTime.HasValue)
                        ? "Present" : "Absent",
                    InTime = s.Attendances
                        .Where(a => a.AttendanceDate.Month == month && a.AttendanceDate.Year == year && a.InTime.HasValue)
                        .Select(a => a.InTime)
                        .FirstOrDefault(),
                    OutTime = s.Attendances
                        .Where(a => a.AttendanceDate.Month == month && a.AttendanceDate.Year == year)
                        .Select(a => a.OutTime)
                        .FirstOrDefault()
                })
                .ToListAsync();


            var activeIdsToday = studentsToday.Where(s => s.StudentStatus == "Active").Select(s => s.StudentId).ToList();


            var allDepartments = await _context.Departments.ToListAsync();

            var departmentsToday = allDepartments.Select(d =>
            {
                var studentsInDept = studentsToday.Where(s => s.DepartmentName == d.DepartmentName).ToList();
                var activeStudents = studentsInDept.Where(s => activeIdsToday.Contains(s.StudentId)).ToList();

                return new DepartmentAttendanceDto
                {
                    DepartmentName = d.DepartmentName,
                    Count = activeStudents.Count, // total students
                    StudentNames = activeStudents.Select(s => s.StudentName).ToList()
                };
            }).ToList();

            var departmentsMonthly = allDepartments.Select(d =>
            {
                var studentsInDept = studentsMonthly.Where(s => s.DepartmentName == d.DepartmentName).ToList();
                var monthlyPresentStudents = studentsInDept.Where(s => s.StudentStatus == "Present").ToList();

                return new DepartmentAttendanceDto
                {
                    DepartmentName = d.DepartmentName,
                    Count = monthlyPresentStudents.Count, // total students in dept
                    StudentNames = monthlyPresentStudents.Select(s => s.StudentName).ToList()
                };
            }).ToList();

            var summary = new AttendanceResultDto
            {
                ActiveCount = activeIdsToday.Count,
                InactiveCount = totalStudents - activeIdsToday.Count,
                ActiveNames = studentsToday.Where(s => s.StudentStatus == "Active").Select(s => s.StudentName).ToList(),
                InactiveNames = studentsToday.Where(s => s.StudentStatus == "Inactive").Select(s => s.StudentName).ToList(),

                MonthlyActiveCount = studentsMonthly.Count(s => s.StudentStatus == "Present"),
                MonthlyInactiveCount = studentsMonthly.Count(s => s.StudentStatus == "Absent"),
                MonthlyActiveNames = studentsMonthly.Where(s => s.StudentStatus == "Present").Select(s => s.StudentName).ToList(),
                MonthlyInactiveNames = studentsMonthly.Where(s => s.StudentStatus == "Absent").Select(s => s.StudentName).ToList(),

                Departments = departmentsToday,
                MonthlyDepartments = departmentsMonthly
            };

            var deptList = allDepartments.Select(d => d.DepartmentName).ToList();

            return new IndexViewModel
            {
                TotalStudents = totalStudents,
                StudentsAttendance = studentsToday,
                MonthlyStudentsAttendance = studentsMonthly,
                AttendanceSummary = summary,
                Departments = deptList
            };
        }

        public async Task<AttendanceResultDto> GetAttendanceByDepartmentAsync(string departmentName)
        {
            var today = DateTime.Today;
            var month = today.Month;
            var year = today.Year;

            var students = await _context.StudentsPortalInfos
                .Include(s => s.Department)
                .Include(s => s.Attendances)
                .Where(s => string.IsNullOrEmpty(departmentName)
                    || s.Department.DepartmentName == departmentName)
                .ToListAsync();

            // ===== TODAY =====
            var activeToday = students
                .Where(s => s.Attendances.Any(a =>
                    a.AttendanceDate == today && a.InTime != null))
                .ToList();

            var inactiveToday = students.Except(activeToday).ToList();

            // ===== MONTHLY =====
            var activeMonthly = students
                .Where(s => s.Attendances.Any(a =>
                    a.AttendanceDate.Month == month &&
                    a.AttendanceDate.Year == year &&
                    a.InTime != null))
                .ToList();

            var inactiveMonthly = students.Except(activeMonthly).ToList();


            return new AttendanceResultDto
            {
                // Today
                ActiveCount = activeToday.Count,
                InactiveCount = inactiveToday.Count,
                ActiveNames = activeToday.Select(s => s.StudentName).ToList(),
                InactiveNames = inactiveToday.Select(s => s.StudentName).ToList(),

                MonthlyActiveCount = activeMonthly.Count,
                MonthlyInactiveCount = inactiveMonthly.Count,
                MonthlyActiveNames = activeMonthly.Select(s => s.StudentName).ToList(),
                MonthlyInactiveNames = inactiveMonthly.Select(s => s.StudentName).ToList()
            
            };
        }


    }
}


