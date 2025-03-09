using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Department
{
    public class DepartmentUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public int? CollegeId { get; set; }
        public string? SubAdmin {  get; set; }
    }
}
