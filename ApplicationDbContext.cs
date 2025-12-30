using Microsoft.EntityFrameworkCore;
using Students_Portal_App.Models.Entities;

namespace Students_Portal_App.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<StudentsPortalInfos> StudentsPortalInfos { get; set; }
        public DbSet<StudentsPaper> StudentsPapers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API: Make RegisterNumber unique
            modelBuilder.Entity<StudentsPortalInfos>(entity =>
            {
                entity.HasIndex(e => e.RegisterNumber).IsUnique();

                // Optional: additional configurations
                entity.Property(e => e.RegisterNumber)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.StudentName)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.StudentStatus)
                      .IsRequired();

                entity.Property(e => e.Department)
                      .HasMaxLength(50)
                      .IsRequired();
            });
        }
    }
}
