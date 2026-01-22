using System.ComponentModel.DataAnnotations;

namespace Students_Portal_App.ViewModels
{
    public class Add_EditStudentVm
    {
        public int StudentId { get; set; }
        [Required]
        public string? RegisterNumber { get; set; }
        [Required]
        public string? StudentName { get; set; }
        [Required]
        public int? DepartmentId { get; set; }
        public string? PhotoPath { get; set; }
    }
}
