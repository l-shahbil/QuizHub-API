using QuizHub.Models.DTO.Authentication;

namespace QuizHub.Services.Authentication.Login_And_Sing_Up.Interface
{
    public interface IAuthenService
    {
        Task<JwtResponse> authenticationAsync(LoginDto userDto);
    }
}
