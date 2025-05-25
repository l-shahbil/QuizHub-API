using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.OpenApi.Writers;
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
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<Exam> _examRepo;
        private readonly IRepository<ExamQuestion> _examQuestionRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<UserDepartment> _userDepartmentRepo;
        private readonly IRepository<Batch> _batchRepo;

        public DeleteService(IRepository<AppUser> studentRepo, IRepository<StudentClass> studentClass, IRepository<StudentExam> studentExamRepo,
            IRepository<StudentAnswers> studentAnswerRepo,IRepository<StudentClass> studentClassRepo, UserManager<AppUser> userManager,
            IRepository<Notification> notificationRepo, IRepository<Class> classRepo, IRepository<ClassExam> classExamRepo,
            IRepository<Question> questionRepo,IRepository<Exam>examRepo,IRepository<ExamQuestion>examQuestionRepo,
            IRepository<Department> departmentRepo,IRepository<UserDepartment>userDepartmentRepo,IRepository<Batch> batchRepo)
        {
            _studentRepo = studentRepo;
            _studentExamRepo = studentExamRepo;
            _studentAnswerRepo = studentAnswerRepo;
           _studentClassRepo = studentClassRepo;
           _userManager = userManager;
            _notificationRepo = notificationRepo;
            _classRepo = classRepo;
            _classExamRepo = classExamRepo;
            _questionRepo = questionRepo;
            _examRepo = examRepo;
            _examQuestionRepo = examQuestionRepo;
            _departmentRepo = departmentRepo;
            _userDepartmentRepo = userDepartmentRepo;
            _batchRepo = batchRepo;
        }

        public async Task deleteSudent(AppUser user)
        {
            try
            {

            var student = await _studentRepo.GetFirstOrDefaultAsync(
                  std => std.Id == user.Id,
                  include: query => query.Include(s => s.StudentClasses).Include(s => s.StudentAnswers)
                  .Include(s => s.studentExams)
                  .Include(s=> s.userDepartments),
                  asNoTracking: false
              );

            List<StudentClass> stdClasses = student.StudentClasses.ToList();
            List<StudentAnswers> stdAnswers = student.StudentAnswers.ToList();
            List<StudentExam> stdExams = student.studentExams.ToList();



            _studentAnswerRepo.RemoveRange(stdAnswers);
            _studentExamRepo.RemoveRange(stdExams);
                _studentClassRepo.RemoveRange(stdClasses);

                foreach(UserDepartment ud in student.userDepartments)
                {
                    _userDepartmentRepo.DeleteEntity(ud);
                }

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
        public async Task deleteExam(Exam exam)
        {
            var exm = await _examRepo.GetFirstOrDefaultAsync(
                     ex => ex.Id == exam.Id,
                     include: query => query.Include(ex => ex.ExamQuestions)
                     .Include(ex => ex.studentAnswers)
                     .Include(ex => ex.classExams)
                                           .ThenInclude(ce => ce.StudentExam).ThenInclude(se => se.studentAnswers),
                     asNoTracking: false
                 );
            var allExamQuestions = exm.ExamQuestions;
            var allClsExams = exm.classExams;
            var studentExams = exm.classExams.SelectMany(ce => ce.StudentExam);
            var allAnswers = studentExams.SelectMany(se => se.studentAnswers);


            _studentAnswerRepo.RemoveRange(allAnswers);
            _studentExamRepo.RemoveRange(studentExams);
            _classExamRepo.RemoveRange(allClsExams);
            _examQuestionRepo.RemoveRange(allExamQuestions);
            _examRepo.DeleteEntity(exm);
        }
        public async Task deleteBatch(Batch batch)
        {
            Batch btch = await _batchRepo.GetIncludeById(batch.Id, "Students");
            foreach (AppUser student in btch.Students)
            {
                await deleteSudent(student);
            }
            _batchRepo.DeleteEntity(batch);
        }
        public async Task deleteDepartment(Department department)
        {
            Department dept = await _departmentRepo.GetFirstOrDefaultAsync(
            filter:d=> d.Id == department.Id,
                include:query=> query.Include(d=> d.Batches)
                                .Include(d=> d.Classes)
                );

            List<Batch> bts = department.Batches.ToList();
            List<Class> classes = department.Classes.ToList();

            foreach (Batch bt in bts)
            {
                await deleteBatch(bt);
            }

            foreach (Class cls in classes)
            {
                await deleteClass(cls);
            }
        }
       
    }
}
