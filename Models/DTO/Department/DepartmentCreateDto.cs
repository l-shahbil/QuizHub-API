using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Department
{
    public class DepartmentCreateDto
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int collegeId { get; set; }
        [Required]
        public string userName { get; set; }
    }
}
