using QuizHub.Models.DTO.Authentication;
using QuizHub.Models.DTO.User;

namespace QuizHub.Services.Authentication.Login_And_Sing_Up.Interface
{
    public interface IAuthenService
    {
        Task<JwtResponse> authenticationAsync(LoginDto userDto);
        Task<ShowMe> showMe(string userEmail);
        Task<bool> resetPassword(string userEmail, ResetPasswordDto model);
    }
}
