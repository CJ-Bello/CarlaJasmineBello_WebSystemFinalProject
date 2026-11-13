using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Models;

namespace StudentGradeManagement.API.Data
{
    public class ApplicationIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationIdentityDbContext(
        DbContextOptions<ApplicationIdentityDbContext> options)
        : base(options)
        {
        }
    }
}
