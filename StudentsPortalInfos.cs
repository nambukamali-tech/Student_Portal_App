using System.ComponentModel.DataAnnotations;

namespace Students_Portal_App.Models.Entities
{
    public class StudentsPortalInfos
    {
        [Key]
        public int StudentId { get; set; }
        [Required(ErrorMessage = "Student Register Number is Required")]
        public string? RegisterNumber { get; set; }

        [Required(ErrorMessage = "Student Status is required")]
        public string? StudentStatus { get; set; }

        [Required(ErrorMessage = "Student Name is required")]
        public string? StudentName { get; set; }
        [Required(ErrorMessage = "Department is required")]
        public string? Department { get; set; }
        [Required(ErrorMessage = "InTime is required")]
        public DateTime? InTime { get; set; }
        [Required(ErrorMessage = "OutTime is required")]
        public DateTime? OutTime { get; set; }     
        public string? PhotoPath { get; set; }
        //Navigation property for related papers ie) one to many relationship 
        public ICollection<StudentsPaper> Papers { get; set; } = new List<StudentsPaper>();

    }
}
