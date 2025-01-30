using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.College
{
    public class CollegeDto
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
