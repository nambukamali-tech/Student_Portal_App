using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Students_Portal_App.Models.Entities
{
    public class StudentsPaper
    {
            [Key]
            public int PaperId { get; set; }
            [Required]
            public string? PaperTitle { get; set; }
            [Required]
            public string? PaperDescription { get; set; }
            [Required]
            public string? PaperCode { get; set; }
            [Required]
            public int StudentId { get; set; }     
          
            // Navigation property should NOT be required
            public StudentsPortalInfos? Student { get; set; }   
    }
}
