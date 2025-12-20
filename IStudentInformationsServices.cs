using Students_Portal_App.Models.Entities;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Interfaces
{
    public interface IStudentInformationsServices
    {
        Task<List<StudentsPortalInfos>> GetStudentsPortalInfosAsync();
        Task<StudentsPortalInfos> AddStudentsPortalInfosAsync(StudentsPortalInfos studentsPortalInfos);
        Task UpdateStudentsPortalInfosAsync(StudentsPortalInfos studentsPortalInfos);
        Task DeleteStudentsPortalInfosAsync(int studentId);
        Task<StudentsPortalInfos?> GetStudentByIdAsync(int studentId);
        Task<StudentsIndexViewModel> GetStudentsDashboardAsync();
        Task UploadStudentPhotoAsync(int studentId, IFormFile photo);

    }
}
