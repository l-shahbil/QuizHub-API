using QuizHub.Models;

namespace QuizHub.Utils.Interface
{
    public interface IDeleteService
    {
        Task deleteSudent(AppUser user);
        Task deleteClass(Class classs);
        Task deleteExam(Exam ex);
        Task deleteBatch(Batch batch);
        Task deleteDepartment(Department department);
    }
}
