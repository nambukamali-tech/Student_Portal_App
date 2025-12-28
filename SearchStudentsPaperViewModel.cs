using Students_Portal_App.Models.Entities;

namespace Students_Portal_App.ViewModels
{
    public class SearchStudentsPaperViewModel
    {
          // Search inputs
            public string? StudentName { get; set; }
            public string? RegisterNumber { get; set; }

            // Result
            public StudentsPortalInfos? Student { get; set; }
            public List<StudentsPaper> StudentsPapers { get; set; } = new();
        }
    }


