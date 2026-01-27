using Students_Portal_App.Data;
using Students_Portal_App.Interfaces;
using Students_Portal_App.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Students_Portal_App.ViewModels;

public class StudentService : IStudentServiceInterface
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public StudentService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<Add_EditStudentVm?> AddStudentAsync(Add_EditStudentVm model, IFormFile? photo)
    {
        var student = new StudentsPortalInfos
        {
            RegisterNumber = model.RegisterNumber,
            StudentName = model.StudentName!,
            DepartmentId = model.DepartmentId!.Value
        };

        // Handle photo
        if (photo != null)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "students");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(photo.FileName);
            var fileName = $"student_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            student.PhotoPath = "/uploads/students/" + fileName;
        }

        _context.StudentsPortalInfos.Add(student);
        await _context.SaveChangesAsync();

        //Before saving the student ! student id is empty  , this line to map entity to viewmodel 
        //studentid generates a value
        //studentid and photopath is auto generated ie) photo path is saved not like image.jpg it have some formats
        //so, the student id and photopath is saved after user enters add student details
        model.StudentId = student.StudentId;
        model.PhotoPath = student.PhotoPath; //map entity back to view model
        return model;
    }

    //Get student by Id
    public async Task<Add_EditStudentVm?> GetStudentByIdAsync(int studentId)
    {
        var student = await _context.StudentsPortalInfos
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null) return null;

        return new Add_EditStudentVm
        {
            StudentId = student.StudentId,
            StudentName = student.StudentName,
            DepartmentId = student.DepartmentId,
            PhotoPath = student.PhotoPath,
            RegisterNumber = student.RegisterNumber
        };
    }
    //APIs are differ based on their request and response format
    //REST APIs uses the JSON format for response

    //Update student details
    public async Task<Add_EditStudentVm?> UpdateStudentAsync(Add_EditStudentVm model, IFormFile? photo)
    {
        var student = await _context.StudentsPortalInfos.FindAsync(model.StudentId);
        if (student == null) return null;

     
        student.StudentName = model.StudentName;
        student.DepartmentId = model.DepartmentId;

        if (photo != null)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "students");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(photo.FileName).ToLower();
            var fileName = $"student_{student.StudentId}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);
            student.PhotoPath = "/uploads/students/" + fileName;
        }

        await _context.SaveChangesAsync();
        //Photopath able to edit because the photo have a option to change
        //In this case, student id is not able to edit
        model.PhotoPath = student.PhotoPath;
        return model;
    }

    //Delete Students service
    //Deleting the students business logics here
    public async Task DeleteStudentAsync(int studentId)
    {
        var student = await _context.StudentsPortalInfos.FindAsync(studentId);//Find the student by studentid
        if (student == null) return;

        _context.StudentsPortalInfos.Remove(student);//Remove the student 
        await _context.SaveChangesAsync();//And save the changes to db
    }

    //Get all students
    public async Task<List<Add_EditStudentVm>> GetAllStudentsAsync()
    {
        var students = await _context.StudentsPortalInfos.ToListAsync();
        return students.Select(s => new Add_EditStudentVm
        {
            StudentId = s.StudentId,
            StudentName = s.StudentName,
            DepartmentId = s.DepartmentId,
            PhotoPath = s.PhotoPath
        }).ToList();
    }

    public async Task<List<Department>> GetDepartmentsAsync() //For showing the Department dropdown list
    {
        return await _context.Departments
            .AsNoTracking()
            .ToListAsync();
    }
}
