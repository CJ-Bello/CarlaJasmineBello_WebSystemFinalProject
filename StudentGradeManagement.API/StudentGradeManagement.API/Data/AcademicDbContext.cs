using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Models;

namespace StudentGradeManagement.API.Data
{
    public class AcademicDbContext : DbContext
    {
        public AcademicDbContext(DbContextOptions<AcademicDbContext> options)
        : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();

        public DbSet<Faculty> Faculty => Set<Faculty>();

        public DbSet<StudentGrade> StudentGrades => Set<StudentGrade>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");

                entity.HasKey(e => e.StudentId);

                entity.Property(e => e.StudentNumber)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.StudentNumber)
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });

            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.ToTable("Faculty");

                entity.HasKey(e => e.FacultyId);

                entity.Property(e => e.FacultyNumber)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.FacultyNumber)
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });

            modelBuilder.Entity<StudentGrade>(entity =>
            {
                entity.ToTable("StudentGrades", table =>
                {
                    table.HasCheckConstraint(
                        "CK_StudentGrades_Grade",
                        "[Grade] BETWEEN 0 AND 100");
                });

                entity.HasKey(e => e.GradeId);

                entity.Property(e => e.SubjectCode)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.SubjectName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Remarks)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.Semester)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.SchoolYear)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.DatePosted)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.DateModified)
                    .HasColumnType("datetime2(0)");

                entity.HasOne(e => e.Student)
                    .WithMany(e => e.Grades)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Faculty)
                    .WithMany(e => e.Grades)
                    .HasForeignKey(e => e.FacultyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.StudentId);

                entity.HasIndex(e => e.FacultyId);

                entity.HasIndex(e => new
                {
                    e.StudentId,
                    e.SubjectCode,
                    e.Semester,
                    e.SchoolYear
                }).IsUnique();
            });
        }
    }
}
