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
           Task<bool> IsTheTeacherLinkedToTheSubject(string teacherId, int classId, int subjectId);
    }
}
