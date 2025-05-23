using QuizHub.Models;

namespace QuizHub.Utils.Interface
{
    public interface IDeleteService
    {
        Task deleteSudent(AppUser user);
        Task deleteClass(Class classs);
    }
}
