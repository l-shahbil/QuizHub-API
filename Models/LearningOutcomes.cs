using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace QuizHub.Models
{
    public class LearningOutcomes
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Title { get; set; }
        public string Description { get; set; }

        //Foreign key
        public int subjectId { get; set; }

        //Relationships
        [ForeignKey("subjectId")]
        public Subject Subject { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
