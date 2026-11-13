using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using StudentGradeManagement.API.Data;
using StudentGradeManagement.API.DTOs;

namespace StudentGradeManagement.API.Controllers
{
    [Route("api/student/grades")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentGradesController : ControllerBase
    {
        private readonly AcademicDbContext _context;

        public StudentGradesController(AcademicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyGrades()
        {
            var studentIdClaim = User.FindFirstValue("StudentId");

            if (!int.TryParse(studentIdClaim, out var studentId))
            {
                return Forbid();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s =>
                    s.StudentId == studentId && s.IsActive);

            if (student == null)
            {
                return NotFound(new
                {
                    message = "Student record not found."
                });
            }

            var grades = await _context.StudentGrades
                .Where(g => g.StudentId == studentId)
                .OrderBy(g => g.SubjectCode)
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

            var dashboard = new StudentDashboardDto
            {
                StudentNumber = student.StudentNumber,
                StudentName = student.FirstName + " " + student.LastName,
                Grades = grades
            };

            return Ok(dashboard);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMyGrade(int id)
        {
            var studentIdClaim = User.FindFirstValue("StudentId");

            if (!int.TryParse(studentIdClaim, out var studentId))
            {
                return Forbid();
            }

            var grade = await _context.StudentGrades
                .Where(g =>
                    g.GradeId == id &&
                    g.StudentId == studentId)
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
                return NotFound(new
                {
                    message = "Grade record not found."
                });
            }

            return Ok(grade);
        }
    }
}
