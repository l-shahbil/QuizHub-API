using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.User.SubAdmin
{
    public class UpdateSubAdminDto
    {
        [EmailAddress]
        public string? Email { get; set; }
        [MinLength(8)]
        public string? PassWord { get; set; }
        [StringLength(50)]
        public string? FirstName { get; set; }
        [StringLength(100)]
        public string? LastName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

    }
}
