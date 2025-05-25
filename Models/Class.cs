using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{

    public class Class
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }

        //Foreignkey
        public string TeacherId { get; set; }

        public int DepartmentId { get; set; }
        public int? SubjectId { get; set; }

        //Relationships
        public ICollection<StudentClass> StudentClasses { get; set; }
        [ForeignKey("TeacherId")]
        public AppUser? Teacher { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<ClassExam> ClassExam { get; set; }
        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }
    }
}
