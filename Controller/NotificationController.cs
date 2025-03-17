using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Constant;
using QuizHub.Models.DTO.Notification;
using QuizHub.Services.Shared_Services.Interface;
using System.Security.Claims;

namespace QuizHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

   
        [HttpPost("send")]
        [Authorize("Permission.Notification.Create")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationCreateDto model, [FromQuery] int departmentId, [FromQuery] int classId)
        {
       
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (userEmail == null) return Unauthorized("Invalid token.");

                var notification = await _notificationService.SendNotificationAsync(userEmail, departmentId, classId, model);
                return Ok(notification);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while sending the notification.", details = ex.Message });
            }
        }

        [HttpPut("edit/{notificationId}")]
        [Authorize("Permission.Notification.Edit")]

        public async Task<IActionResult> EditNotification(int notificationId, [FromBody] NotificationEditDto model)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (userEmail == null) return Unauthorized("Invalid token.");

                var updatedNotification = await _notificationService.EditNotificationAsync(userEmail, notificationId, model);
                return Ok(updatedNotification);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while editing the notification.", details = ex.Message });
            }
        }

        [HttpDelete("delete/{notificationId}")]
        [Authorize("Permission.Notification.Delete")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (userEmail == null) return Unauthorized("Invalid token.");

                bool isDeleted = await _notificationService.DeleteNotificationAsync(userEmail, notificationId);
                return isDeleted ? Ok(new { message = "Notification deleted successfully." }) : BadRequest("Failed to delete notification.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the notification.", details = ex.Message });
            }
        }

        [Authorize("Permission.Notification.View")]
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetAllNotificationsInClass(int classId)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (userEmail == null) return Unauthorized("Invalid token.");

                var notifications = await _notificationService.GetAllNotificationsInClassAsync(userEmail, classId);
                return Ok(notifications);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching notifications.", details = ex.Message });
            }
        }
    }
}
