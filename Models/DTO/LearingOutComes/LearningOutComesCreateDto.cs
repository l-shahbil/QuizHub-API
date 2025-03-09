
using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.LearingOutComes
{
    public class LearningOutComesCreateDto
    {
        [Required, StringLength(50)]
        public string Title { get; set; }
        public string Description { get; set; }

    }
}
