using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class StudentClass
    {
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class Class { get; set; }
    }
}
