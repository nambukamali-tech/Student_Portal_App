using Students_Portal_App.DTOs;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Interfaces
{
    public interface IAttendanceService
    {
         Task<IndexViewModel> GetIndexAsync(string statusType);

         Task<AttendanceResultDto> MarkAttendanceAsync(int studentId, DateTime? InTime, DateTime? OutTime);
    }
}
