using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class Notification
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]

        public DateTime CreateAt { get; set; }
        

        //Foreign key
        public string userId { get; set; }
        public int classId { get; set; }

        //Relationships
        [ForeignKey("userId")]
        public AppUser User { get; set; }
        [ForeignKey("classId")]
        public Class Class { get; set; }

    }
}
