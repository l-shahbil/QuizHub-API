namespace QuizHub.Models.DTO.Class
{
    public class ClassUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TeacherEmail { get; set; }

        public int? SubjectId { get; set; }

    }
}
