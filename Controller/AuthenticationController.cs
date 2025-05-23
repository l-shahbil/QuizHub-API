using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuizHub.Models;
using QuizHub.Models.DTO.Authentication;
using QuizHub.Models.DTO.User;
using QuizHub.Services.Authentication.Login_And_Sing_Up.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenService _authenticationService;

        public AuthenticationController(IAuthenService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto userDto)
        {
            if (ModelState.IsValid)
            {

                var jwtResponse = await _authenticationService.authenticationAsync(userDto);
                if (jwtResponse != null) {

                    return Ok(new
                    {
                        token = jwtResponse.Token,
                        userType = jwtResponse.userType,
                        expiration = jwtResponse.Expiration
                    });
                }
            }
            return Unauthorized();
        }
        [Authorize]
        [HttpGet("ShowMe")]
        public async Task<IActionResult> ShowMe()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email).Value;
            ShowMe userShowMe = await _authenticationService.showMe(userEmail);

            return Ok(userShowMe);
        }

        [Authorize] 
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userEmail = User.FindFirst(ClaimTypes.Email).Value;

            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(new { error = "User email not found in token." });

            try
            {
                var result = await _authenticationService.resetPassword(userEmail, model);
                return Ok(new { message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}

