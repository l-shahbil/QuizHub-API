using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Department
{
    public class DepartmentDto
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
