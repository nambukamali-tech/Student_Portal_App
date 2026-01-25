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
        //Business logic for Mark Attendance page
        public async Task<AttendanceResultDto> MarkAttendanceAsync(int studentId, DateTime? inTime, DateTime? outTime)
        {
            var today = DateTime.Today;

            // Add or update today's attendance
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

            // Datas made here and send to mark attendance page then used for students details display

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
        //Business logic for Index page
        public async Task<IndexViewModel> GetIndexAsync()
        {
            var today = DateTime.Today;
            var month = today.Month;
            var year = today.Year;

            // Total students from table (studentsportalinfos)
            int totalStudents = await _dbAccessorService.GetTotalstudentCount();//Asynchronously count the total students

            // Active students today
            var activeIdsToday = await _context.Attendances
                .Where(a => a.AttendanceDate == today && a.InTime != null)
                .Select(a => a.StudentId)
                
                .ToListAsync();

            int activeCount = activeIdsToday.Count;
            int inactiveCount = totalStudents - activeCount;

            // Load all students with their attendance for today
            var students = await _context.StudentsPortalInfos
      .Include(s => s.Department)
      .Include(s => s.Attendances)
      .Select(s => new StudentsAttendanceDto
      {
          StudentId = s.StudentId,
          PhotoPath = s.PhotoPath,
          StudentName = s.StudentName!,
          DepartmentName = s.Department != null ? s.Department.DepartmentName : "",
          RegisterNumber = s.RegisterNumber!,

          // Compute status from attendance today
          StudentStatus = s.Attendances
              .Any(a => a.AttendanceDate == today && a.InTime.HasValue)
              ? "Active"
              : "Inactive",

          // InTime / OutTime only if attendance exists today
          InTime = s.Attendances
              .Where(a => a.AttendanceDate == today)
              .Select(a => a.InTime)
              .FirstOrDefault(),
          OutTime = s.Attendances
              .Where(a => a.AttendanceDate == today)
              .Select(a => a.OutTime)
              .FirstOrDefault()
      })
      .ToListAsync();

            // Monthly attendance
            var monthlyPresentIds = await _context.Attendances
                .Where(a => a.AttendanceDate.Month == month && a.AttendanceDate.Year == year && a.InTime != null)
                .Select(a => a.StudentId)
                .Distinct()
                .ToListAsync();

            int monthlyPresentCount = monthlyPresentIds.Count;
            int monthlyAbsentCount = totalStudents - monthlyPresentCount;

            // Departments Today
            // Get all departments from DB
            var allDepartments = await _context.Departments.ToListAsync();

            // Departments Today (include all, even if 0 active)
            var departmentsToday = allDepartments.Select(d =>
            {
                var studentsInDept = students.Where(s => s.DepartmentName == d.DepartmentName).ToList();
                var activeStudents = studentsInDept.Where(s => activeIdsToday.Contains(s.StudentId)).ToList();

                return new DepartmentAttendanceDto
                {
                    DepartmentName = d.DepartmentName,
                    Count = activeStudents.Count,
                    StudentNames = activeStudents.Select(s => s.StudentName).ToList()
                };
            }).ToList();

            // Departments Monthly (similar logic)
            var departmentsMonthly = allDepartments.Select(d =>
            {
                var studentsInDept = students.Where(s => s.DepartmentName == d.DepartmentName).ToList();
                var monthlyPresentStudents = studentsInDept.Where(s => monthlyPresentIds.Contains(s.StudentId)).ToList();

                return new DepartmentAttendanceDto
                {
                    DepartmentName = d.DepartmentName,
                    Count = monthlyPresentStudents.Count,
                    StudentNames = monthlyPresentStudents.Select(s => s.StudentName).ToList()
                };
            }).ToList();


            // Attendance Summary
            var summary = new AttendanceResultDto
            {
                ActiveCount = activeCount,
                InactiveCount = inactiveCount,
                ActiveIds = activeIdsToday,
                ActiveNames = students.Where(s => activeIdsToday.Contains(s.StudentId)).Select(s => s.StudentName).ToList(),
                InactiveNames = students.Where(s => !activeIdsToday.Contains(s.StudentId)).Select(s => s.StudentName).ToList(),

                MonthlyPresentCount = monthlyPresentCount,
                MonthlyAbsentCount = monthlyAbsentCount,
                MonthlyPresentIds = monthlyPresentIds,
                Departments = departmentsToday,           // Current
                MonthlyPresentNames = students
    .Where(s => monthlyPresentIds.Contains(s.StudentId))
    .Select(s => s.StudentName)
    .ToList(),

                MonthlyAbsentNames = students
    .Where(s => !monthlyPresentIds.Contains(s.StudentId))
    .Select(s => s.StudentName)
    .ToList(),

                MonthlyDepartments = departmentsMonthly   // Monthly
            };
            return new IndexViewModel
            {

                TotalStudents = totalStudents,
                StudentsAttendance = students,
                AttendanceSummary = summary
            };
        }
    }
}


