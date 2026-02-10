using Students_Portal_App.Data;
using Students_Portal_App.Interfaces;
using Students_Portal_App.Models.Entities;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Services
{
    public class StudentDashboardService : IDashboardService
    {
        private readonly StudentsListService _studentsService;

        public StudentDashboardService(StudentsListService studentsService)
        {
            _studentsService = studentsService;
        }

        public async Task<StudentDashboardVm?> GetStudentDashboardAsync(string regno, string? courseFilter = null)
        {
            if (string.IsNullOrWhiteSpace(regno))
                return null;

            var dashboardData = await _studentsService.GetStudentDashboardAsync(regno);

            if (dashboardData == null)
                return null;

            dashboardData.Papers ??= new List<DepartmentPapers>();
            dashboardData.Scholarships ??= new List<StudentScholarship>();
            dashboardData.MonthlyAttendance ??= new List<Attendance>();
            dashboardData.Courses = new List<string> { "BSc", "MSc", "PhD" };

            if (!string.IsNullOrWhiteSpace(courseFilter))
            {
                dashboardData.Papers = dashboardData.Papers
                    .Where(p => p.CourseName == courseFilter)
                    .ToList();
            }

            dashboardData.SelectedCourse = courseFilter;
            dashboardData.RegNo = regno;
            dashboardData.PresentCount = dashboardData.MonthlyAttendance.Count(a => a.InTime != null);
            dashboardData.AbsentCount = dashboardData.MonthlyAttendance.Count(a => a.InTime == null);

            return dashboardData;
        }
    }
}
