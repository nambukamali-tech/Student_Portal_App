using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Students_Portal_App.Data;
using Students_Portal_App.Models.Entities;

namespace Students_Portal_App.Controllers
{
   
    public class VwStudentsportalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VwStudentsportalController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetDataView()//Take the datas from Mysql views 
        {
            //Views helps to avoid writing joins again and again
            //Views not store the data it stores the query (logic of joins)
            var data = await _context.VwStudentsportal.ToListAsync();
            //ToListAsync()-> Without Blocking List the datas
            return View(data);//Return view ie) datas stored in data "variable"
        }
    }
}
