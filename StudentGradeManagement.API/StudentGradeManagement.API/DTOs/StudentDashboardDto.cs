namespace StudentGradeManagement.API.DTOs
{
    public class StudentDashboardDto
    {
        public string StudentNumber { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public List<GradeResponseDto> Grades { get; set; } = new();
    }
}
