using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Models;

namespace StudentGradeManagement.API.Data
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var academicContext = scope.ServiceProvider
                .GetRequiredService<AcademicDbContext>();

            var password = configuration["SeedAccounts:Password"];

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Seed account password is missing from User Secrets.");
            }

            string[] roles = { "Faculty", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Could not create role {role}: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            var students = await academicContext.Students
                .Where(s => s.IsActive)
                .ToListAsync();

            foreach (var student in students)
            {
                var email = student.Email.Trim();
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        StudentId = student.StudentId
                    };

                    var createResult = await userManager.CreateAsync(
                        user, password);

                    if (!createResult.Succeeded)
                    {
                        throw new Exception(
                            $"Could not create student account {email}: " +
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    }
                }
                else if (user.StudentId != student.StudentId ||
                         user.FacultyId.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Existing account {email} has an incorrect academic mapping.");
                }

                await EnsureRoleAsync(userManager, user, "Student");
            }

            var facultyMembers = await academicContext.Faculty
                .Where(f => f.IsActive)
                .ToListAsync();

            foreach (var faculty in facultyMembers)
            {
                var email = faculty.Email.Trim();
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FacultyId = faculty.FacultyId
                    };

                    var createResult = await userManager.CreateAsync(
                        user, password);

                    if (!createResult.Succeeded)
                    {
                        throw new Exception(
                            $"Could not create faculty account {email}: " +
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    }
                }
                else if (user.FacultyId != faculty.FacultyId ||
                         user.StudentId.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Existing account {email} has an incorrect academic mapping.");
                }

                await EnsureRoleAsync(userManager, user, "Faculty");
            }
        }

        private static async Task EnsureRoleAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string expectedRole)
        {
            var currentRoles = await userManager.GetRolesAsync(user);

            if (currentRoles.Any(r => r != expectedRole))
            {
                throw new InvalidOperationException(
                    $"Account {user.Email} already has a conflicting role.");
            }

            if (!currentRoles.Contains(expectedRole))
            {
                var result = await userManager.AddToRoleAsync(
                    user, expectedRole);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Could not assign role {expectedRole} to {user.Email}: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
