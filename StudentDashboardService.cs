using Students_Portal_App.Data;
using Students_Portal_App.ViewModels;
using Students_Portal_App.Interfaces;

namespace Students_Portal_App.Services
{
    public class StudentDashboardService : IDashboardService
    {
        private readonly StudentsService _studentsService;

        public StudentDashboardService(StudentsService studentsService)
        {
            _studentsService = studentsService;
        }
        public async Task<StudentDashboardVm?> GetStudentDashboardAsync(string regno)
        {
            if (string.IsNullOrEmpty(regno))
                return null;

            var dashboardData = await _studentsService.GetStudentDashboardAsync(regno);
            if (dashboardData == null)
                return null;

            // Calculate Present & Absent for showing the dashboard page
            dashboardData.PresentCount = dashboardData.MonthlyAttendance
                .Count(a => a.InTime != null);

            dashboardData.AbsentCount = dashboardData.MonthlyAttendance
                .Count(a => a.InTime == null);

            return dashboardData;
        }
    }
}
