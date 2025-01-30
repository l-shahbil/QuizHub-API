using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class UserDepartment
    {
        public string userId { get; set; }
        [ForeignKey("userId")]
        public AppUser User { get;set; }

        public int departmentId { get; set; }
        [ForeignKey("departmentId")]
        public Department Department { get; set; }
    }
}
