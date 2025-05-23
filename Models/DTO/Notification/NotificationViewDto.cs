namespace QuizHub.Models.DTO.Notification
{
    public class NotificationViewDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string senderEmail { get; set; }
        public string senderName { get; set; }
        public DateTime sendTime { get; set; }
    }
}
