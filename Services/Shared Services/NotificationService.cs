using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Notification;
using QuizHub.Services.Shared_Services.Interface;

namespace QuizHub.Services.Shared_Services
{
    public class NotificationService : INotificationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Notification> _notificationRepo;
        private readonly IRepository<Class> _calssRepo;

        public NotificationService(UserManager<AppUser> userManager, IRepository<Department> departmentRepo, IRepository<Notification> notificationRepo, IRepository<Class> calssRepo)
        {
            _userManager = userManager;
            _departmentRepo = departmentRepo;
            _notificationRepo = notificationRepo;
            _calssRepo = calssRepo;
        }
        public async Task<NotificationViewDto> SendNotificationAsync(string userEmail, int departmentId, int classId, NotificationCreateDto model)
        {
                Class cls = await _calssRepo.GetByIdAsync(classId);
                if (cls == null || cls.DepartmentId != departmentId)
                {
                    throw new KeyNotFoundException($"A Class with ID {classId} not found.");
                }

                var user = await _userManager.FindByEmailAsync(userEmail);
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(Roles.SubAdmin.ToString()))
                {
                    var existDepartment = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
                    if (existDepartment == null)
                    {
                        throw new KeyNotFoundException($"A Department with ID {departmentId} not found.");
                    }
                    if (existDepartment.SubAdmin.Email != userEmail)
                    {
                        throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
                    }

                    Notification notification = new Notification()
                    {
                        Title = model.Title,
                        Content = model.Content,
                        CreateAt = DateTime.Now,
                        User = user,
                        Class = cls
                    };

                    await _notificationRepo.AddAsyncEntity(notification);
                    return new NotificationViewDto
                    {

                        Id = notification.Id,
                        Title = notification.Title,
                        Content = notification.Content,
                        sendTime = notification.CreateAt,
                        senderEmail = userEmail
                        ,senderName= $"{user.FirstName} {user.LastName}"
                    };

                }
                else if (roles.Contains(Roles.Teacher.ToString()))
                {
                    if (cls.TeacherId != user.Id)
                    {
                        throw new UnauthorizedAccessException("Access Denied: You are not the assigned Teacher for this Class.");

                    }
                    Notification notification = new Notification()
                    {
                        Title = model.Title,
                        Content = model.Content,
                        CreateAt = DateTime.Now,
                        User = user,
                        Class = cls
                    };

                    await _notificationRepo.AddAsyncEntity(notification);
                    return new NotificationViewDto
                    {

                        Id = notification.Id,
                        Title = notification.Title,
                        Content = notification.Content,
                        sendTime = notification.CreateAt,
                        senderEmail = userEmail
                    };
                }

                    return null;

        }
        public async Task<NotificationViewDto> EditNotificationAsync(string userEmail, int notificationId, NotificationEditDto model)
        {
            AppUser user = await _userManager.FindByEmailAsync(userEmail);
            Notification notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null )
            {
                throw new KeyNotFoundException($"A Notification with ID {notificationId} not found.");

            }
            if (notification.userId != user.Id)
            {
                throw new UnauthorizedAccessException("You are not allowed to delete this notification because you are not the sender.");
            }

            notification.Title = string.IsNullOrWhiteSpace(model.Title) ? notification.Title : model.Title;
            notification.Content = string.IsNullOrWhiteSpace(model.Content) ? notification.Content : model.Content;
            _notificationRepo.UpdateEntity(notification);

            return new NotificationViewDto
            {
                Id = notification.Id,
              Title = notification.Title,
                Content = notification.Content,
                senderEmail = userEmail,
                senderName = $"{user.FirstName} {user.LastName}"
                ,
                sendTime = notification.CreateAt
            };
        }
        public async Task<bool> DeleteNotificationAsync(string userEmail, int notificationId)
        {
            AppUser user = await _userManager.FindByEmailAsync(userEmail);
            Notification notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null ) 
            {
                throw new KeyNotFoundException($"A Notification with ID {notificationId} not found.");

            }
            if (notification.userId != user.Id)
            {
                throw new UnauthorizedAccessException("You are not allowed to delete this notification because you are not the sender.");
            }

            _notificationRepo.DeleteEntity(notification);
            return true;
        }


        public async Task<List<NotificationViewDto>> GetAllNotificationsInClassAsync(string userEmail, int classId)
        {
            Class cls = await _calssRepo.GetIncludeById(classId, "StudentClasses");
            if (cls == null)
            {
                throw new KeyNotFoundException($"A Class with ID {classId} not found.");
            }
            var department = await _departmentRepo.GetByIdAsync(cls.DepartmentId);
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                if (!user.departments.Contains(department))
                {
                    throw new ArgumentException("Not Found Class.");
                }
                var notification = _notificationRepo.GetAllIncludeAsync("User").Result.Where(n => n.classId == classId)
                    .Select(n => new NotificationViewDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Content = n.Content,
                        sendTime = n.CreateAt,
                        senderEmail = n.User.Email,
                        senderName = $"{n.User.FirstName} {n.User.LastName}"

                    }).ToList();
                return notification;
            }
            else if(roles.Contains(Roles.Teacher.ToString())){
                if(cls.TeacherId != user.Id) {
                    throw new ArgumentException("Not Found Class.");
                }
                var notification = _notificationRepo.GetAllIncludeAsync("User").Result.Where(n => n.classId == classId)
                       .Select(n => new NotificationViewDto
                       {
                           Id = n.Id,
                           Title = n.Title,
                           Content = n.Content,
                           sendTime = n.CreateAt,
                           senderEmail = n.User.Email,
                           senderName = $"{n.User.FirstName} {n.User.LastName}"

                       }).ToList();
                return notification;
            }
            else if (roles.Contains(Roles.Student.ToString())){
                if (cls.StudentClasses.Any(sc => sc.UserId == user.Id)) 
                {
                    var notification = _notificationRepo.GetAllIncludeAsync("User").Result.Where(n => n.classId == classId)
                        .Select(n => new NotificationViewDto
                        {
                            Id = n.Id,
                            Title = n.Title,
                            Content = n.Content,
                            sendTime = n.CreateAt,
                            senderEmail = n.User.Email,
                            senderName = $"{n.User.FirstName} {n.User.LastName}"

                        }).ToList();
                    return notification;
                }
                throw new UnauthorizedAccessException("The student is not authorized to access this class.");

            }
            return null;
        }
    }
}
