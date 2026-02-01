using Microsoft.AspNetCore.Mvc;
using Students_Portal_App.Interfaces;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Controllers
{
    public class StaffController : Controller
    {
        private readonly IStudentServiceInterface _studentService;
        public StaffController(IStudentServiceInterface studentService)
        {
            _studentService = studentService;
        }
        public async Task<IActionResult> StaffDashboard()
        {
            StaffDashboardViewModel model = await _studentService.GetStaffDashboardAsync();
            return View(model);
        }

    }
}
