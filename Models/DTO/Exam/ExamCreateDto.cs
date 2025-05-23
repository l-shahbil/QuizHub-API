using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Exam
{
    public class ExamCreateDto
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public int SubjectId { get; set; }
        [Required]
       public List<int> learningOutcomeIds { get; set; }
        [Required]
       public int NumberOfDifficultQuestions { get; set; }
        [Required]
       public int NumberOfMediumLevelQuestions { get; set; }
        [Required]
       public int NumberOfEasyQuestions { get; set; }
        [Required]
        public decimal ClarityRangeFrom { get; set; }
        [Required]
        public decimal ClarityRangeTo { get; set; }
    
        public int classId { get; set; }

    }
}
