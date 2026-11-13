using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentGradeManagement.API.Data;
using StudentGradeManagement.API.DTOs;
using StudentGradeManagement.API.Models;


namespace StudentGradeManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AcademicDbContext _academicContext;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            AcademicDbContext academicContext,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _academicContext = academicContext;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    Message = "Invalid email or password."
                });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(
                user, request.Password);

            if (!passwordValid)
            {
                return Unauthorized(new
                {
                    Message = "Invalid email or password."
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Count != 1)
            {
                return StatusCode(403, new
                {
                    Message = "Account must have exactly one assigned role."
                });
            }

            var role = roles[0];
            string fullName;
            var additionalClaims = new List<Claim>();

            if (role == "Student" && user.StudentId.HasValue)
            {
                var student = await _academicContext.Students
                    .FirstOrDefaultAsync(s =>
                        s.StudentId == user.StudentId.Value &&
                        s.IsActive);

                if (student == null)
                {
                    return StatusCode(403, new
                    {
                        Message = "Student account is inactive or not linked."
                    });
                }

                fullName = $"{student.FirstName} {student.LastName}";

                additionalClaims.Add(
                    new Claim("StudentId", student.StudentId.ToString()));
            }
            else if (role == "Faculty" && user.FacultyId.HasValue)
            {
                var faculty = await _academicContext.Faculty
                    .FirstOrDefaultAsync(f =>
                        f.FacultyId == user.FacultyId.Value &&
                        f.IsActive);

                if (faculty == null)
                {
                    return StatusCode(403, new
                    {
                        Message = "Faculty account is inactive or not linked."
                    });
                }

                fullName = $"{faculty.FirstName} {faculty.LastName}";

                additionalClaims.Add(
                    new Claim("FacultyId", faculty.FacultyId.ToString()));
            }
            else
            {
                return StatusCode(403, new
                {
                    Message = "Account role and academic record do not match."
                });
            }

            var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier, user.Id),

            new Claim(
                ClaimTypes.Email, user.Email ?? string.Empty),

            new Claim(
                ClaimTypes.Name, fullName),

            new Claim(
                ClaimTypes.Role, role)
        };

            claims.AddRange(additionalClaims);

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new LoginResponseDto
            {
                Token = tokenString,
                Email = user.Email ?? string.Empty,
                Role = role,
                FullName = fullName
            });
        }
    }
}
