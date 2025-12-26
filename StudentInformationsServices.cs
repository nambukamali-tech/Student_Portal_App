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
        private readonly IWebHostEnvironment _webHostEnvironment;//Essential to web root path for save the images
        //constructor to initialize the ApplicationDbContext instance using Dependency Injection
        public StudentInformationsServices(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;//Dependency Injection for WebHostEnvironment to access wwwroot folder
        }

        public async Task<List<StudentsPortalInfos>> GetStudentsPortalInfosAsync()//Get Students Service
        {
            return await _context.StudentsPortalInfos.ToListAsync();
        }
        //Add Students Service (business logic) for student portal
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
                //here using Linq to query data from database using orderby and select 
                .OrderBy(s => s.StudentName)//Using Orderby to sort the students by name
                .Select(static s => new StudentsListDtos//selssect specific fields using dtos
                {
                    StudentId = s.StudentId,
                    StudentStatus = s.StudentStatus,
                    StudentName = s.StudentName,
                    Department = s.Department,
                    InTime = s.InTime,
                    OutTime = s.OutTime,
                    PhotoPath = s.PhotoPath
                })
                .ToListAsync();
            return new StudentsIndexViewModel
            {
                Summary = summary,
                Students = students
            };
        }

        //Upload Student Photo Service
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

        //View Student by Id Service
        public async Task<StudentsPortalInfos?> GetStudentByIdAsync(int studentId)
        {
            return await _context.StudentsPortalInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }
      
        //Add details to paper
        public async Task<StudentsPaper> AddPaperAsync(StudentsPaper studentsPaper)
        {
            await _context.StudentsPapers.AddAsync(studentsPaper);
            await _context.SaveChangesAsync();
            return studentsPaper;
        }

        //show the joining tables with combined student and papers table
        [HttpGet]
        public async Task<List<StudentsPaper>> GetStudentswithPapersAsync()
        {
            return await _context.StudentsPapers
                .AsNoTracking()
                .ToListAsync();
        }

        //Get Papers + Students Details
        public async Task<List<StudentsPaperdtos>> GetStudentsWithPapersAsync(int page, int pageSize)
        {
            return await (
                from s in _context.StudentsPortalInfos
                join p in _context.StudentsPapers
                    on s.StudentId equals p.StudentId
                orderby s.StudentName
                select new StudentsPaperdtos
                {
                    StudentId = s.StudentId,
                    StudentName = s.StudentName!,
                    RegisterNumber = s.RegisterNumber!,
                    Department = s.Department!,
                    PaperId = p.PaperId,
                    PaperCode = p.PaperCode!,
                    PaperTitle = p.PaperTitle!,
                    PaperDescription = p.PaperDescription!
                }
            )
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
        }

        public async Task<int> GetStudentsWithPaperCountAsync()
        {
            return await (
                from s in _context.StudentsPortalInfos
                join p in _context.StudentsPapers
                    on s.StudentId equals p.StudentId
                select p.PaperId
            ).CountAsync();
        }


        public Task<List<StudentsPaper>> GetStudentsPapersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<StudentsPaperdtos>> GetStudentsWithPapersAsync()
        {
            throw new NotImplementedException();
        }
    }

}

