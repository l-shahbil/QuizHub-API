using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Class
{
    public class ClassCreateDto
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
