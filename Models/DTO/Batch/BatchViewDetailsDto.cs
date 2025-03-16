using QuizHub.Models.DTO.User.Student;

namespace QuizHub.Models.DTO.Batch
{
    public class BatchViewDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<StudentViewDto> Students { get; set; }
    }
}
