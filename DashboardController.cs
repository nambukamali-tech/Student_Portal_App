using Microsoft.AspNetCore.Mvc;
using Students_Portal_App.Interfaces;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(string regno, string courseFilter)
        {
            StudentDashboardVm dashboardVm;

            if (!string.IsNullOrWhiteSpace(regno))
            {
                dashboardVm = await _dashboardService.GetStudentDashboardAsync(regno, courseFilter);
            }
            else
            {
                dashboardVm = new StudentDashboardVm
                {
                    Courses = new List<string> { "BSc", "MSc", "PhD" }
                };
            }

            return View(dashboardVm);
        }

    }
}
