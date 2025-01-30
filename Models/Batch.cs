using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class Batch
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }

        //foreign key
        public int DepartmentId { get; set; }
        //Relationships
        public ICollection<AppUser> Students { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }
}
