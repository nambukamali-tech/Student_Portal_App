namespace Students_Portal_App.DTOs
{
    public class StudentsSummaryDtos
    {
        //students summary dtos only for index view model not stored in database
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }

    }
}
