using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using StudentGradeManagement.API.Data;
using StudentGradeManagement.API.DTOs;
using StudentGradeManagement.API.Models;
using StudentGradeManagement.API.Services;

namespace StudentGradeManagement.API.Controllers
{
    [Route("api/faculty/grades")]
    [ApiController]
    [Authorize(Roles = "Faculty")]
    public class FacultyGradesController : ControllerBase
    {
        private readonly AcademicDbContext _context;
        private readonly GradeRemarksService _remarksService;

        public FacultyGradesController(
            AcademicDbContext context,
            GradeRemarksService remarksService)
        {
            _context = context;
            _remarksService = remarksService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGrades()
        {
            var grades = await _context.StudentGrades
                .Include(g => g.Student)
                .Include(g => g.Faculty)
                .OrderBy(g => g.Student.LastName)
                .ThenBy(g => g.SubjectCode)
                .Select(g => new GradeResponseDto
                {
                    GradeId = g.GradeId,
                    StudentId = g.StudentId,
                    StudentNumber = g.Student.StudentNumber,
                    StudentName = g.Student.FirstName + " " + g.Student.LastName,
                    SubjectCode = g.SubjectCode,
                    SubjectName = g.SubjectName,
                    FacultyName = g.Faculty.FirstName + " " + g.Faculty.LastName,
                    Grade = g.Grade,
                    Remarks = g.Remarks,
                    Semester = g.Semester,
                    SchoolYear = g.SchoolYear,
                    DatePosted = g.DatePosted,
                    DateModified = g.DateModified
                })
                .ToListAsync();

            return Ok(grades);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetGrade(int id)
        {
            var grade = await _context.StudentGrades
                .Include(g => g.Student)
                .Include(g => g.Faculty)
                .Where(g => g.GradeId == id)
                .Select(g => new GradeResponseDto
                {
                    GradeId = g.GradeId,
                    StudentId = g.StudentId,
                    StudentNumber = g.Student.StudentNumber,
                    StudentName = g.Student.FirstName + " " + g.Student.LastName,
                    SubjectCode = g.SubjectCode,
                    SubjectName = g.SubjectName,
                    FacultyName = g.Faculty.FirstName + " " + g.Faculty.LastName,
                    Grade = g.Grade,
                    Remarks = g.Remarks,
                    Semester = g.Semester,
                    SchoolYear = g.SchoolYear,
                    DatePosted = g.DatePosted,
                    DateModified = g.DateModified
                })
                .FirstOrDefaultAsync();

            if (grade == null)
            {
                return NotFound(new { message = "Grade record not found." });
            }

            return Ok(grade);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGrade(CreateGradeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var facultyIdClaim = User.FindFirstValue("FacultyId");

            if (!int.TryParse(facultyIdClaim, out var facultyId))
            {
                return Forbid();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == dto.StudentId && s.IsActive);

            if (student == null)
            {
                return NotFound(new { message = "Active student not found." });
            }

            var duplicate = await _context.StudentGrades.AnyAsync(g =>
                g.StudentId == dto.StudentId &&
                g.SubjectCode == dto.SubjectCode &&
                g.Semester == dto.Semester &&
                g.SchoolYear == dto.SchoolYear);

            if (duplicate)
            {
                return Conflict(new
                {
                    message = "A grade record already exists for this student, subject, semester, and school year."
                });
            }

            var grade = new StudentGrade
            {
                StudentId = dto.StudentId,
                SubjectCode = dto.SubjectCode.Trim(),
                SubjectName = dto.SubjectName.Trim(),
                FacultyId = facultyId,
                Grade = dto.Grade,
                Remarks = _remarksService.CalculateRemarks(dto.Grade),
                Semester = dto.Semester.Trim(),
                SchoolYear = dto.SchoolYear.Trim(),
                DatePosted = DateTime.UtcNow
            };

            _context.StudentGrades.Add(grade);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetGrade),
                new { id = grade.GradeId },
                new { message = "Grade created successfully.", gradeId = grade.GradeId }
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateGrade(int id, UpdateGradeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var facultyIdClaim = User.FindFirstValue("FacultyId");

            if (!int.TryParse(facultyIdClaim, out var facultyId))
            {
                return Forbid();
            }

            var grade = await _context.StudentGrades
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
            {
                return NotFound(new { message = "Grade record not found." });
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == dto.StudentId && s.IsActive);

            if (student == null)
            {
                return NotFound(new { message = "Active student not found." });
            }

            var duplicate = await _context.StudentGrades.AnyAsync(g =>
                g.GradeId != id &&
                g.StudentId == dto.StudentId &&
                g.SubjectCode == dto.SubjectCode &&
                g.Semester == dto.Semester &&
                g.SchoolYear == dto.SchoolYear);

            if (duplicate)
            {
                return Conflict(new
                {
                    message = "Another grade record already exists for this student, subject, semester, and school year."
                });
            }

            grade.StudentId = dto.StudentId;
            grade.SubjectCode = dto.SubjectCode.Trim();
            grade.SubjectName = dto.SubjectName.Trim();
            grade.Grade = dto.Grade;
            grade.Remarks = _remarksService.CalculateRemarks(dto.Grade);
            grade.Semester = dto.Semester.Trim();
            grade.SchoolYear = dto.SchoolYear.Trim();
            grade.FacultyId = facultyId;
            grade.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Grade updated successfully." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var grade = await _context.StudentGrades
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
            {
                return NotFound(new { message = "Grade record not found." });
            }

            _context.StudentGrades.Remove(grade);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Grade deleted successfully." });
        }
    }
}
