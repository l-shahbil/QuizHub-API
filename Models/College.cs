using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models
{
    public class College
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }


        //Relationships
        public ICollection<Department> Departments { get; set; }

    }
}
