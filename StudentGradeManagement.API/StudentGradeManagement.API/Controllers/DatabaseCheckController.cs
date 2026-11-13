using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Data;

namespace StudentGradeManagement.API.Controllers
{
    [Route("api/database-check")]
    [ApiController]
    public class DatabaseCheckController : ControllerBase
    {
        private readonly AcademicDbContext _context;

        public DatabaseCheckController(AcademicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var students = await _context.Students.CountAsync();
                var faculty = await _context.Faculty.CountAsync();
                var grades = await _context.StudentGrades.CountAsync();

                return Ok(new
                {
                    Message = "Database connection successful!",
                    Database = "StudentGradeManagementDB",
                    Students = students,
                    Faculty = faculty,
                    StudentGrades = grades
                });
            }
            catch
            {
                return StatusCode(503, new
                {
                    Message = "Database connection failed. Check your SQL Server configuration."
                });
            }
        }
    }
}
