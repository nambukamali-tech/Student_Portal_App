using Students_Portal_App.Models.Entities;

namespace Students_Portal_App.ViewModels
{
    public class SearchDepartmentPapersViewModel
    {
          // Search inputs
            public string? DepartmentName { get; set; }
            public string? DepartmentId { get; set; }
            
            public string? CourseName { get; set; }

        //Result
        
        public List<DepartmentPapers>? Papers { get; set; }

           
        }
    }


