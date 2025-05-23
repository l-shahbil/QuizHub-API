using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Utils.Interface;

namespace QuizHub.Utils
{
    public class DeleteService:IDeleteService
    {
        private readonly IRepository<AppUser> _studentRepo;
        private readonly IRepository<StudentExam> _studentExamRepo;
        private readonly IRepository<StudentAnswers> _studentAnswerRepo;
        private readonly IRepository<StudentClass> _studentClassRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Notification> _notificationRepo;
        private readonly IRepository<Class> _classRepo;
        private readonly IRepository<ClassExam> _classExamRepo;

        public DeleteService(IRepository<AppUser> studentRepo, IRepository<StudentClass> studentClass, IRepository<StudentExam> studentExamRepo, IRepository<StudentAnswers> studentAnswerRepo,
            IRepository<StudentClass> studentClassRepo, UserManager<AppUser> userManager, IRepository<Notification> notificationRepo, IRepository<Class> classRepo, IRepository<ClassExam> classExamRepo)
        {
            _studentRepo = studentRepo;
            _studentExamRepo = studentExamRepo;
            _studentAnswerRepo = studentAnswerRepo;
           _studentClassRepo = studentClassRepo;
           _userManager = userManager;
            _notificationRepo = notificationRepo;
            _classRepo = classRepo;
            _classExamRepo = classExamRepo;
        }

        public async Task deleteSudent(AppUser user)
        {
            try
            {

            var student = await _studentRepo.GetFirstOrDefaultAsync(
                  std => std.Id == user.Id,
                  include: query => query.Include(s => s.StudentClasses).Include(s => s.StudentAnswers)
                  .Include(ex => ex.studentExams),
                  asNoTracking: false
              );

            List<StudentClass> stdClasses = student.StudentClasses.ToList();
            List<StudentAnswers> stdAnswers = student.StudentAnswers.ToList();
            List<StudentExam> stdExams = student.studentExams.ToList();



            _studentAnswerRepo.RemoveRange(stdAnswers);
            _studentExamRepo.RemoveRange(stdExams);
                //_userDepartmentRepo.DeleteEntity(IsStudentInDepartment);
                _studentClassRepo.RemoveRange(stdClasses);

            await _userManager.DeleteAsync(user);

            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task deleteClass(Class classs)
        {
            Class cls = await _classRepo.GetFirstOrDefaultAsync(
             cls => cls.Id == classs.Id,
             include: query => query.Include(cls => cls.StudentClasses).Include(cls => cls.Notifications)
             .Include(cls => cls.ClassExam).ThenInclude(clsEx => clsEx.StudentExam).ThenInclude(stdExam => stdExam.studentAnswers),
             asNoTracking: false
         );

            List<ClassExam> clsExams = cls.ClassExam.ToList();
            List<StudentExam> studentExams = clsExams.SelectMany(ce=> ce.StudentExam).ToList();
            List<StudentAnswers> studentAnswers = studentExams.SelectMany(se => se.studentAnswers).ToList();


            _notificationRepo.RemoveRange(cls.Notifications);
            _studentClassRepo.RemoveRange(cls.StudentClasses);
            _studentAnswerRepo.RemoveRange(studentAnswers);
            _studentExamRepo.RemoveRange(studentExams);
            _classExamRepo.RemoveRange(clsExams);

            _classRepo.DeleteEntity(classs);
        }
    }
}
