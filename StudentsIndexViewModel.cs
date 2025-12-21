using Students_Portal_App.DTOs;

namespace Students_Portal_App.ViewModels
{
    //ViewModel only for Index View to show summary and List of students
    public class StudentsIndexViewModel
    {
        public required StudentsSummaryDtos Summary { get; set; }
        public required List<StudentsListDtos> Students { get; set; }
        
    }
}
