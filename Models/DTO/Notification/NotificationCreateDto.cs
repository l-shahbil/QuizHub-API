using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Notification
{
    public class NotificationCreateDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
