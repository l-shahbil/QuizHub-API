using Microsoft.AspNetCore.Identity;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Services.Shared_Services.Interface;

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

        public ExamValidator(UserManager<AppUser> userManager, IRepository<Exam> examRepo, IRepository<Question> questionRepo,
            IRepository<Subject> subjectRepo, IRepository<Department> departmentRepo, IRepository<Class> calssRepo,
            IRepository<LearningOutcomes> learningOutComesRepo, IRepository<Class> classRepo)
        {
            _userManager = userManager;
            _examRepo = examRepo;
            _questionRepo = questionRepo;
            _subjectRepo = subjectRepo;
            _departmentRepo = departmentRepo;
            _calssRepo = calssRepo;
            _learningOutComesRepo = learningOutComesRepo;
            _classRepo = classRepo;
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
            var isSubjectiAsseignedToDepartment = existDepartment.Subjects.Any(s => s.Id == subjectId);
            if (!isSubjectiAsseignedToDepartment)
            {
                throw new Exception("The subject not assigned to department.");
            }
            return true;
        }

  

        public async Task<bool> IsTheTeacherLinkedToTheSubject(string teacherId, int classId,int subjectId)
        {
            var cls = await _calssRepo.GetByIdAsync(classId);
                if (cls.SubjectId != subjectId || cls.TeacherId != teacherId)
                {
                    throw new InvalidOperationException("The teacher is not assigned to the specified subject or class.");
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
    }
}
