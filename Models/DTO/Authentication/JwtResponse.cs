namespace QuizHub.Models.DTO.Authentication
{
    public class JwtResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }

    }
}
