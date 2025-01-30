using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models
{
    public class Subject
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }


        //Relationship
        public ICollection<Department> Departments { get; set; }
        public ICollection<Class> Classes { get; set; }
        public ICollection<LearingOutcomes> LearingOutcomes { get; set; }
        public ICollection<Exam> Exams { get; set; }

    }
}
