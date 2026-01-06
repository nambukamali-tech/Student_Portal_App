namespace Students_Portal_App.Models.Entities
{
    public class VwStudentsportal
    {
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? RegisterNumber { get; set; }
        public string? StudentStatus { get; set; }
        public string? Department { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }

        //Studentspapers details
        public string? PaperTitle { get; set; }
        public string? PaperCode { get; set; }
     
    }
}
