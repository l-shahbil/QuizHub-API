using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Services.Shared_Services.Interface;
using System.ComponentModel;

namespace QuizHub.Services.Shared_Services
{
    public class ExamValidator : IExamValidator
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Exam> _examRepo;
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Class> _calssRepo;
        private readonly IRepository<LearningOutcomes> _learningOutComesRepo;
        private readonly IRepository<Class> _classRepo;
        private readonly IRepository<StudentClass> _studentClassRepo;
        private readonly IRepository<ClassExam> _classExamRepo;
        private readonly IRepository<StudentExam> _studentExamRepo;

        public ExamValidator(UserManager<AppUser> userManager, IRepository<Exam> examRepo, IRepository<Question> questionRepo,
            IRepository<Subject> subjectRepo, IRepository<Department> departmentRepo, IRepository<Class> calssRepo,
            IRepository<LearningOutcomes> learningOutComesRepo, IRepository<Class> classRepo,IRepository<StudentClass> studentClassRepo,
            IRepository<ClassExam> classExamRepo, IRepository<StudentExam> studentExamRepo)
        {
            _userManager = userManager;
            _examRepo = examRepo;
            _questionRepo = questionRepo;
            _subjectRepo = subjectRepo;
            _departmentRepo = departmentRepo;
            _calssRepo = calssRepo;
            _learningOutComesRepo = learningOutComesRepo;
            _classRepo = classRepo;
            _studentClassRepo = studentClassRepo;
            _classExamRepo = classExamRepo;
            _studentExamRepo = studentExamRepo;
        }

        public async Task<bool> CheckSubAdminOwnershipDepartment(string userEmail, int departmentId)
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
            return true;
        }

        public async Task<bool> IsLearningOutcomesFounded(int subjectId, List<int> learningOutComesIDs)
        {

            Subject subject =await _subjectRepo.GetIncludeById(subjectId, "LearingOutcomes");
            List<int> learningOutcomes = subject.LearingOutcomes.Select(lr => lr.Id).ToList();
            foreach (var lrID in learningOutComesIDs)
            {
                if (!learningOutcomes.Contains(lrID))
                {
                    throw new Exception($"Learning outcome with ID {lrID} does not exist.");
                }
            }
            return true;
        }

        public async Task<bool> IsSubjectAssignedToDepartment(int subjectId, int departmentId)
        {
            var existDepartment = await _departmentRepo.GetIncludeById(departmentId, "Subjects");
            if (existDepartment != null)
            {
                throw new KeyNotFoundException("department is not found.");
            }
            var isSubjectiAsseignedToDepartment = existDepartment.Subjects.Any(s => s.Id == subjectId);
            if (!isSubjectiAsseignedToDepartment)
            {
                throw new Exception("The subject not assigned to department.");
            }
            return true;
        }


        //int? classId....why? Because the function is not called unless it is the teacher role, if it is the supervisor role, the classID will be null.
        public async Task<bool> IsTheTeacherLinkedToTheSubject(string teacherId, int? classId,int subjectId)
        {
            var cls = await _calssRepo.GetByIdAsync(classId);
                if (cls.SubjectId != subjectId || cls.TeacherId != teacherId)
                {
                    throw new InvalidOperationException("The teacher is not assigned to the specified subject or class.");
                }
            return true;
        }
        public async Task<bool> IsTheTeacherLinkedToTheSubject(string teacherId, int subjectId)
        {
            Subject subject = await _subjectRepo.GetIncludeById(subjectId, "Classes");
            Class cls = subject.Classes.FirstOrDefault(cls => cls.TeacherId == teacherId);
            if(cls == null)
            {

            
            throw new InvalidOperationException("The teacher is not attached to the subject.");
        }
            
            return true;
        }


        public async Task<bool> IsSubjectAssignedToTheClass(int subjectId, int classId)
        {
            Class cls = await _calssRepo.GetByIdAsync(classId);

            if (cls.SubjectId != subjectId)
            {
                throw new InvalidOperationException($"The subject with ID {subjectId} is not assigned to the class with ID {classId}.");
            }

            return true;
        }


        public async Task<bool> IsTheClassInThisDepartment(int departmentId, int classId)
        {
            var classs = await _classRepo.GetIncludeById(classId);
            if (classs == null || classs.DepartmentId != departmentId)
            {
                throw new InvalidOperationException($"Class with ID {classId} was not found.");

            }
            return true;
        }

        public async Task<bool> CheckSubAdminOwnershipClass(string subAdminId, int classId)
        {
            Class cls = await _classRepo.GetIncludeById(classId, "Department");

            if (cls == null)
            {
                throw new InvalidOperationException($"Class with ID {classId} was not found.");
            }
           

            Department department = cls.Department;

            if (subAdminId != department.subAdminId)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not authorized to manage this class.");
            }

            return true;


        }
        public void validateDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new InvalidOperationException("Start date must be before end date.");
            }
        }
        public void validateNonNegativeForScore(decimal score) 
        {
            if(score < 0)
            {
                throw new InvalidOperationException($"{score} cannot be negative.");
            }
        }

        public async Task<bool> isTheTeacherLinkedToTheClass(string userEmail, int classId)
        {
            var cls = await _calssRepo.GetByIdAsync(classId);
            var teacher = await _userManager.FindByEmailAsync(userEmail);

            if (cls.TeacherId != teacher.Id)
            {
                throw new InvalidOperationException("The teacher is not assigned to the specified class.");
            }
            return true;
        }

        public async Task<bool> isTheStudentLinkedToTheClass(string userEmail, int classId)
        {
            var cls = await _calssRepo.GetByIdAsync(classId);
            var student = await _userManager.FindByEmailAsync(userEmail);
            var clsStd = await _studentClassRepo.GetAllAsync();
            var classStudent = clsStd.Where(clsStd=> clsStd.UserId == student.Id).Where(cls => cls.ClassId == classId);

            if (classStudent.Count()==0)
            {
                throw new InvalidOperationException($"Student with email '{userEmail}' is not enrolled in the specified class (ID: {classId}).");
            }
            return true;
        }

        public async Task<ClassExam> isTheExamPublishedInTheClass(string examId, int classId, ExamFunctionType funcType)
        {

            var clsExs = await _classExamRepo.GetAllAsync();
            var clasExam = clsExs.FirstOrDefault(ce => ce.ExamId == examId && ce.ClassId== classId);

            if (funcType == ExamFunctionType.publishExam)
            {

                if (clasExam != null ) 
                {
                    if(clasExam.EndTime >= DateTime.Now)
                    {
                    throw new InvalidOperationException($"The exam has been published on this class.");
                    }
                   
                }
            }
            else if(funcType == ExamFunctionType.cancelExam)
            {
                if (clasExam == null)
                {
                    throw new InvalidOperationException($"The exam has not been published on this class.");
                }
                return clasExam;
            }
            else if(funcType == ExamFunctionType.takeExam)
            {
                if (clasExam == null)
                {
                    throw new InvalidOperationException($"The exam has not been published on this class.");
                }
                return clasExam;
            }

            return null;
        }
        public bool isTheExamAvalible(ClassExam clsExam)
        {
            if (clsExam.EndTime < DateTime.Now)
            {
                throw new InvalidOperationException("The exam is no longer available. The allowed time has expired.");
            }
            return true;
        }

        public async Task<bool> HasStudentTakenExamAsync(string userId, int classId, string examId)
        {
            var stdExs = await _studentExamRepo.GetAllAsync();
            StudentExam stdEx = stdExs.FirstOrDefault(se => se.userId == userId && (se.clsExam.ClassId == classId && se.clsExam.ExamId == examId));
            if (stdEx != null)
            {
                throw new InvalidOperationException("The student has already taken this exam.");
            }
            return true;
        }

        public async Task<bool> checkDateAndDuration(StudentExam stdExam,TimeSpan durationStudent)
        {
            var clsExams = await _classExamRepo.GetAllAsync();
            ClassExam clsExam = clsExams.FirstOrDefault(ce=> ce.ExamId == stdExam.clsExamExamId && ce.ClassId == stdExam.clsExamClassId)!;

            var examEnd = clsExam.EndTime + clsExam.Duration;
            if (examEnd < DateTime.Now)
            {
                throw new InvalidOperationException("The exam period has ended. You can no longer access this exam.");
            }

            if(durationStudent > clsExam.Duration)
            {
                throw new InvalidOperationException("You have exceeded the allowed time for the exam. Your attempt is no longer valid.");
            }

            return true;
        }

    }
}
