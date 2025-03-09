using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Department
{
    public class DepartmentViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CollegeName { get; set; }
        public string SubAdmin { get; set; }
    }
}
