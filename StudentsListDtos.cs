using System.ComponentModel.DataAnnotations;

namespace Students_Portal_App.DTOs
{
    public class StudentsListDtos
    {
        [Key]
        public int StudentId { get; set; }

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

    }
}
