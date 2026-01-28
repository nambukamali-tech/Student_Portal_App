using Microsoft.AspNetCore.Mvc;
using Students_Portal_App.Interfaces;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Controllers
{
    public class PaperController : Controller
    {
        private readonly IPaperServiceInterface _paperService;

        public PaperController(IPaperServiceInterface paperService)
        {
            _paperService = paperService;
        }

        [HttpGet]
        public async Task<IActionResult> AddPaper()
        {
            var model = await _paperService.GetPaperFormViewModelAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPaper(PaperFormViewmodel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _paperService.AddPaperAsync(model.Paper);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Paper added successfully!" : "Failed to add paper!";

            return RedirectToAction(nameof(AddPaper));
        }

        [HttpGet]
        public async Task<IActionResult> ViewPaper()
        {
            var papers = await _paperService.GetAllPapersViewModelAsync();
            return View(papers);
        }
    }
}
