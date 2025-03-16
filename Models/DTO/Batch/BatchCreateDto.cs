using System.ComponentModel.DataAnnotations;
using QuizHub.Models;

namespace QuizHub.Models.DTO.Batch
{
    public class BatchCreateDto
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
