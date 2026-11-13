using Microsoft.AspNetCore.Identity;

namespace StudentGradeManagement.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? StudentId { get; set; }

        public int? FacultyId { get; set; }
    }
}
