namespace Students_Portal_App.DTOs
{
    public class StudentsPaperdtos
    {
        public int PaperId { get; set; }
        public required string?  PaperTitle { get; set; }
        public required string? PaperDescription { get; set; }
        public required string? PaperCode { get; set; }
        public int StudentId { get; set; }
        public required string? StudentName { get; set; }
        public required string? RegisterNumber { get; set; }
        public required string Department { get;  set; }
    }
    
}
