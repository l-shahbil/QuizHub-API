using QuizHub.Constant;
using QuizHub.Models;

namespace QuizHub.Services.Shared_Services.Interface
{
    public interface IExamValidator
    {
            Task<bool> IsSubjectAssignedToDepartment(int subjectId, int departmentId);
            Task<bool> IsSubjectAssignedToTheClass(int subjectId, int classId);
            Task<bool> IsLearningOutcomesFounded(int subjectId,List<int> learningOutComesIDs);
            Task<bool> CheckSubAdminOwnershipDepartment(string userEmail, int departmentId);
            Task<bool> CheckSubAdminOwnershipClass(string subAdminId, int classId);
            Task<bool> IsTheClassInThisDepartment(int departmentId, int classId);
           Task<bool> IsTheTeacherLinkedToTheSubject(string teacherId, int? classId, int subjectId);
        Task<bool> IsTheTeacherLinkedToTheSubject(string teacherId, int subjectId);
           void validateDateRange(DateTime startDate, DateTime endDate);
            void validateNonNegativeForScore(decimal score);
        Task<bool> isTheTeacherLinkedToTheClass(string userEmail, int classId);
        Task<bool> isTheStudentLinkedToTheClass(string userEmail, int classId);
        Task<ClassExam> isTheExamPublishedInTheClass(string examId, int classId,ExamFunctionType funcType);
        bool isTheExamAvalible(ClassExam clsExam);
        Task<bool> HasStudentTakenExamAsync(string userId, int classId,string examId);
        Task<bool> checkDateAndDuration(StudentExam stdExam, TimeSpan durationStudent);

    }
}
