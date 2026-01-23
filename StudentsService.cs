using Microsoft.EntityFrameworkCore;
using Students_Portal_App.Data;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Services
{
    public class StudentsService 
    {
        private readonly ApplicationDbContext _context;
        public StudentsService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<StudentDashboardVm?> GetStudentDashboardAsync(string regno)
        {
            //Take the students details by using regno
            var student = await _context.StudentsPortalInfos
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.RegisterNumber == regno);
            
            //If student from that regno is null then return null
            if (student == null)
                return null;

            //Take papers details
            var papers = await _context.DepartmentPapers
                .Where(p => p.DepartmentId == student.DepartmentId)
                .ToListAsync();

            //Take scholarship details
            var scholarships = await _context.StudentScholarships
                .Where(s => s.StudentId == student.StudentId)
                .ToListAsync();

            //
            var month = DateTime.Today.Month;
            var year = DateTime.Today.Year;

            //Take Attendance Details
            var attendance = await _context.Attendances
                .Where(a => a.StudentId == student.StudentId
                && a.AttendanceDate.Month == month
                && a.AttendanceDate.Year == year)
                .ToListAsync();

            //return eveything in viewmodel
            return new StudentDashboardVm
            {
                Student = student,
                Papers = papers,
                Scholarships = scholarships,
                MonthlyAttendance = attendance,
                PresentCount = attendance.Count(a => a.InTime != null),
                AbsentCount = attendance.Count(a => a.InTime == null)
            };
        }
    }
}
