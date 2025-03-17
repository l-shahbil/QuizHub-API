using QuizHub.Models.DTO.Notification;

namespace QuizHub.Services.Shared_Services.Interface
{
    public interface INotificationService
    {
        Task<NotificationViewDto> SendNotificationAsync(string userEmail, int departmentId, int classId, NotificationCreateDto model);
        Task<NotificationViewDto> EditNotificationAsync(string userEmail, int notificationId, NotificationEditDto model);
        Task<List<NotificationViewDto>> GetAllNotificationsInClassAsync(string userEmail, int classId);
        Task<bool> DeleteNotificationAsync(string userEmail, int notificationId);
    }
}
