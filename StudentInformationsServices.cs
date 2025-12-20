using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Students_Portal_App.Data;
using Students_Portal_App.DTOs;
using Students_Portal_App.Interfaces;
using Students_Portal_App.Models.Entities;
using Students_Portal_App.ViewModels;

namespace Students_Portal_App.Services
{
    public class StudentInformationsServices : IStudentInformationsServices
    {

        private readonly ApplicationDbContext _context;//Make the instance of ApplicationDbContext with readonly field
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentInformationsServices(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<StudentsPortalInfos>> GetStudentsPortalInfosAsync()//Get Students Service
        {
            return await _context.StudentsPortalInfos.ToListAsync();
        }
        //Add Students Service (business logic)
        public async Task<StudentsPortalInfos> AddStudentsPortalInfosAsync(StudentsPortalInfos studentsPortalInfos)
        {
            _context.StudentsPortalInfos.Add(studentsPortalInfos);
            await _context.SaveChangesAsync();
            return studentsPortalInfos;
            
        }
        //Update Students Service (business logic)
        public async Task UpdateStudentsPortalInfosAsync(StudentsPortalInfos studentsPortalInfos)
        {
            var existingStudent = await _context.StudentsPortalInfos.FindAsync(studentsPortalInfos.StudentId);
            if (existingStudent != null)
            {
                existingStudent.StudentStatus = studentsPortalInfos.StudentStatus;
                existingStudent.StudentName = studentsPortalInfos.StudentName;
                existingStudent.Department = studentsPortalInfos.Department;
                existingStudent.InTime = studentsPortalInfos.InTime;
                existingStudent.OutTime = studentsPortalInfos.OutTime;
                existingStudent.LastPresent_New = studentsPortalInfos.LastPresent_New;
                existingStudent.Actions = studentsPortalInfos.Actions;
                await _context.SaveChangesAsync();

            }


        }
        //Delete Students Service (business logic)
        public async Task DeleteStudentsPortalInfosAsync(int studentId)
        {
            var student =
                await _context.StudentsPortalInfos.FindAsync(studentId);

            if (student != null)
            {
                _context.StudentsPortalInfos.Remove(student);
                await _context.SaveChangesAsync();
            }

        }
        //
        //Dashboard Service using DTOs and ViewModel with LINQ
        public async Task<StudentsIndexViewModel> GetStudentsDashboardAsync()
        {
            var query = _context.StudentsPortalInfos.AsNoTracking();

            var summary = new StudentsSummaryDtos
            {
                TotalStudents = await query.CountAsync(),
                ActiveStudents = await query.CountAsync(s => s.StudentStatus == "Active"),
                InactiveStudents = await query.CountAsync(s => s.StudentStatus != "Active")
            };

            var students = await query
                .OrderBy(s => s.StudentName)
                .Select(static s => new StudentsListDtos
                {
                    StudentId = s.StudentId,
                    StudentStatus = s.StudentStatus,
                    StudentName = s.StudentName,
                    Department = s.Department,
                    InTime = s.InTime,
                    OutTime = s.OutTime,
                    LastPresent_New = s.LastPresent_New,
                    PhotoPath = s.PhotoPath
                })
                .ToListAsync();
            return new StudentsIndexViewModel
            {
                Summary = summary,
                Students = students
            };
        }
        public async Task UploadStudentPhotoAsync(int studentId, IFormFile photo)
        {
            var student = await _context.StudentsPortalInfos.FindAsync(studentId);
            if (student == null)
                throw new Exception("Student not found");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "students-photo");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(photo.FileName).ToLower();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowed.Contains(ext))
                throw new Exception("Invalid image format");

            var fileName = $"student_{studentId}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            student.PhotoPath = "/students-photo/" + fileName;
            await _context.SaveChangesAsync();
        }


       

        public async Task<StudentsPortalInfos?> GetStudentByIdAsync(int studentId)
        {
            return await _context.StudentsPortalInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }




    }
}
