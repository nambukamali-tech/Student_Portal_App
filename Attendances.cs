using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Students_Portal_App.Models.Entities
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public StudentsPortalInfos Student { get; set; } = null!;

        [Required]
        public DateTime AttendanceDate { get; set; }
        //Required field is must to overcome the issue of non-nullable column
        [Required]
        
        public string Status { get; set; } = "Absent";

        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
    }
}
