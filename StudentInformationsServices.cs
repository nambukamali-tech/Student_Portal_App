using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Students_Portal_App.Data;
using Students_Portal_App.DTOs;
using Students_Portal_App.Interfaces;
using Students_Portal_App.Models.Entities;
using Students_Portal_App.ViewModels;
using MySqlConnector;
using System.Data;

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
                //using Linq to query data from database using orderby and select 
                .OrderBy(s => s.StudentName)//Using Orderby to sort the students by name
                .Select(static s => new StudentsListDtos//select specific fields using dtos
                {
                    StudentId = s.StudentId,
                    StudentStatus = s.StudentStatus,
                    StudentName = s.StudentName,
                    Department = s.Department.DepartmentName,
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

        //Just to solve the error
        public async Task<List<DepartmentPapers>> GetStudentsPapersAsync()
        {
            // Fetch all papers from the database
            return await _context.DepartmentPapers
                .Include(p => p.Department) // optional: include department details
                .ToListAsync();
        }

        //Add details to paper
        public async Task<DepartmentPapers> AddPaperAsync(DepartmentPapers studentsPaper)
        {
            await _context.DepartmentPapers.AddAsync(studentsPaper);
            await _context.SaveChangesAsync();
            return studentsPaper;
        }

        //View details of Department papers
        public async Task<List<DepartmentPapersdtos>> GetDepartmentPapersAsync()
        {

            return await _context.DepartmentPapers //Its Db Name incluse the Department Db
                .Include(p => p.Department)
                .Select(p => new DepartmentPapersdtos//Take datas from DepartmentPapersdtos also
                {
                    PaperId = p.PaperId,
                    PaperCode = p.PaperCode,
                    PaperDescription = p.PaperDescription,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department.DepartmentName,
                    CourseName = p.CourseName

                })
                .ToListAsync();

        }

        //show the joining tables with combined student and papers table
        [HttpGet]
        public async Task<List<DepartmentPapers>> GetStudentswithPapersAsync()
        {
            return await _context.DepartmentPapers
                .AsNoTracking()
                .ToListAsync();
        }

        //Get Papers + Students Details
        public async Task<List<StudentsPaperdtos>> GetStudentsWithPapersAsync(int page, int pageSize)
        {
            return await (
                from s in _context.StudentsPortalInfos
                join p in _context.DepartmentPapers
                    on s.DepartmentId equals p.DepartmentId
                orderby s.StudentName
                select new StudentsPaperdtos
                {
                    StudentId = s.StudentId,
                    StudentName = s.StudentName!,
                    RegisterNumber = s.RegisterNumber!,
                    Department = s.Department.DepartmentName!,//Error for department field because of string variable
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
                join p in _context.DepartmentPapers
                    on s.DepartmentId equals p.DepartmentId
                select p.PaperId
            ).CountAsync();
        }
        //Using stored procedure for student scholarship
        public async Task AddScholarshipUsingSP(AddScholarshipViewModel model)
        {
            // Get the connection string from DbContext
            var connString = _context.Database.GetConnectionString();

            using var conn = new MySqlConnection(connString);
            using var cmd = new MySqlCommand("sp_add_student_scholarship", conn);//Connection with stored procedure

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_register_number", model.RegisterNumber);
            cmd.Parameters.AddWithValue("p_scholarship_name", model.ScholarshipName);
            cmd.Parameters.AddWithValue("p_amount", model.Amount);
            cmd.Parameters.AddWithValue("p_dob", model.Dob);
            cmd.Parameters.AddWithValue("p_scholarship_year", model.ScholarshipYear);
            cmd.Parameters.AddWithValue("p_country", model.Country);
            cmd.Parameters.AddWithValue("p_state", model.State);
            cmd.Parameters.AddWithValue("p_district", model.District);
            cmd.Parameters.AddWithValue("p_status", model.Status);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<List<StudentScholarshipdtos>> GetAllScholarshipsAsync()
        {
            return await (
                from s in _context.StudentsPortalInfos
                join sch in _context.StudentScholarships
                    on s.StudentId equals sch.StudentId
                select new StudentScholarshipdtos
                {
                    RegisterNumber = s.RegisterNumber,
                    StudentName = s.StudentName,
                    ScholarshipName = sch.ScholarshipName,
                    Amount = sch.Amount,
                    Dob = sch.Dob,
                    Year = sch.ScholarshipYear,
                    Country = sch.Country,
                    State = sch.State,
                    District = sch.District,
                    Status = sch.Status
                }
            ).AsNoTracking().ToListAsync();
        }   
        Task<List<VwStudentsportal>> IStudentInformationsServices.GetDataView()
        {
            throw new NotImplementedException();
        }

        Task<List<VwStudentsportal>> IStudentInformationsServices.GetDatas()
        {
            throw new NotImplementedException();
        }

        Task<List<StudentScholarshipdtos>> IStudentInformationsServices.GetAllScholarshipsAsync()
        {
            throw new NotImplementedException();
        }

        Task IStudentInformationsServices.AddScholarshipUsingSP(AddScholarshipViewModel model)
        {
            throw new NotImplementedException();
        }

        Task<List<DepartmentPapers>> IStudentInformationsServices.GetStudentsPapersAsync()
        {
            throw new NotImplementedException();
        }

        Task<StudentsPortalInfos> IStudentInformationsServices.AddStudentsPortalInfosAsync(StudentsPortalInfos studentsPortalInfos)
        {
            throw new NotImplementedException();
        }

        Task IStudentInformationsServices.UpdateStudentsPortalInfosAsync(StudentsPortalInfos studentsPortalInfos)
        {
            throw new NotImplementedException();
        }

        Task IStudentInformationsServices.DeleteStudentsPortalInfosAsync(int studentId)
        {
            throw new NotImplementedException();
        }

        Task<StudentsPortalInfos?> IStudentInformationsServices.GetStudentByIdAsync(int studentId)
        {
            throw new NotImplementedException();
        }

        Task<StudentsIndexViewModel> IStudentInformationsServices.GetStudentsDashboardAsync()
        {
            throw new NotImplementedException();
        }

        Task IStudentInformationsServices.UploadStudentPhotoAsync(int studentId, IFormFile photo)
        {
            throw new NotImplementedException();
        }


        Task<int> IStudentInformationsServices.GetStudentsWithPaperCountAsync()
        {
            throw new NotImplementedException();
        }
    }

}

