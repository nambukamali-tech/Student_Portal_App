using System.ComponentModel.DataAnnotations;

namespace Students_Portal_App.Models.Entities
{
    public class StudentsPortalInfos
    {
        [Key]
        public int StudentId { get; set; }
        [Required]
        public required string StudentStatus { get; set; }
        [Required]
        public required string StudentName { get; set; }
        public required string Department { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public DateTime LastPresent_New { get; set; }
        public required string Actions { get; set; }
        public string? PhotoPath { get; set; }


    }
}
