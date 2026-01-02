using Students_Portal_App.Models.Entities;
namespace Students_Portal_App.ViewModels
{
    public class AddScholarshipViewModel
    {
        public required string RegisterNumber { get; set; }
        public  required string ScholarshipName { get; set; }
        public decimal Amount { get; set; }
        public DateTime Dob { get; set; }
        public int ScholarshipYear { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? District { get; set; }
        public string? Status { get; set; }
    }
}
