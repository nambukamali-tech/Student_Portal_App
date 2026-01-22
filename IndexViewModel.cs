using Students_Portal_App.DTOs;

namespace Students_Portal_App.ViewModels
{
    public class IndexViewModel
    {
        //For showing total students count
        public int TotalStudents { get; set; }
        public string StudentStatus { get; set; } = string.Empty;
        //child view models
        public List<StudentRowViewModel> Students { get; set; } = new();

        public List<StudentsAttendanceDto> StudentsAttendance { get; set; } = new();
        public StudentsStatusViewModel StudentsStatus { get; set; } = new();
        public DepartmentDetailsViewModel DepartmentDetails { get; set; } = new();
        public AttendanceResultDto AttendanceSummary { get; set; } = new();

    }
    //For showing summary of students in index page students table
    public class StudentRowViewModel
    {
        public int StudentId { get; set; }
        public string RegisterNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentStatus { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string MonthlyStatus { get; set; } = string.Empty;
        
        public bool HasMonthlyAttendance { get; set; }
    }
    //For showing student details chart
    public class  StudentsStatusViewModel
    {
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }

    }
    //For showing department details chart
    public class  DepartmentDetailsViewModel
    {
        public int HighStrength { get; set; }
        public int MediumStrength { get; set; }
        public int LowStrength { get; set; }
        
    }

}
