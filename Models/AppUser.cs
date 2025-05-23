using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuizHub.Models
{
    public class AppUser:IdentityUser
    {

        [Required,StringLength(50)]
        public string FirstName { get; set; }
        [Required,StringLength(100)]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime RegistraionDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        //Foreign key
        public int? BatchId { get; set; }

        //Relationships
        public ICollection<UserDepartment> userDepartments { get; set; }
        public ICollection<Department> departments { get; set; }
        [ForeignKey("BatchId")]
        public Batch? Batch { get; set; }
        public ICollection <StudentClass> StudentClasses { get;set; }
        //Teaches
        public ICollection<Class> Classes { get; set; }
        public ICollection<StudentAnswers> StudentAnswers { get; set; }
        public ICollection<Exam> Exams { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<StudentExam> studentExams { get; set; }
        public ICollection<Notification> Notifications { get; set; }





    }
}
