using System.ComponentModel.DataAnnotations;

namespace StudentGradeManagement.API.DTOs
{
    public class CreateGradeDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(30)]
        public string SubjectCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string SubjectName { get; set; } = string.Empty;

        [Range(0, 100)]
        public int Grade { get; set; }

        [Required]
        [StringLength(50)]
        public string Semester { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SchoolYear { get; set; } = string.Empty;
    }
}
