using Microsoft.AspNetCore.Mvc;
using Students_Portal_App.Interfaces;
using Students_Portal_App.ViewModels;
using Students_Portal_App.Services;

namespace Students_Portal_App.Controllers
{
    public class ScholarshipController : Controller
    {
        private readonly IStudentScholarshipService _studentScholarshipService;
        public ScholarshipController(IStudentScholarshipService studentScholarshipService)
        {
            _studentScholarshipService = studentScholarshipService;
        }
            [HttpGet]
            public async Task<IActionResult> ScholarshipsList()
            {
                var scholarships = await _studentScholarshipService.GetAllScholarshipsAsync();
                return View(scholarships);
            }

            [HttpGet]
            public IActionResult AddScholarship()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> AddScholarship(AddScholarshipViewModel model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                try
                {
                    await _studentScholarshipService.AddScholarshipUsingSP(model);
                    TempData["Success"] = "Scholarship added successfully!";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

                return RedirectToAction("AddScholarship");
            }            
        }
    }
