using Microsoft.EntityFrameworkCore;
using Student_API.Models.Entities;
namespace Student_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<StudentsInformations> StudentsInformations { get; set; }

    }
}
