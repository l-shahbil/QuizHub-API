using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class Department
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }

        //ForeignKey

        public string? subAdminId { get; set; }
        public int collegeId { get; set; }


        //Relationships
        [ForeignKey("collegeId")]
        public College Colleges { get; set; }
      
        public ICollection<Batch> Batches { get; set; }
        public ICollection<Subject> Subjects { get; set; }
        public ICollection<Class> Classes { get; set; }
        
        public ICollection<UserDepartment> UserDepartments { get; set; }
        // in order Supadmin
        [ForeignKey("subAdminId")]
        public AppUser? SubAdmin { get; set; }

    }
}
