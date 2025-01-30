using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuizHub.Models;
using QuizHub.Models.DTO.Authentication;
using QuizHub.Services.Authentication.Login_And_Sing_Up.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenService _authenticationService;

        public AuthenticationController(IAuthenService authenticationService)
        {
            _authenticationService = authenticationService;
        }

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
                        expiration = jwtResponse.Expiration
                    });
                }
            }
            return Unauthorized();
        }
    }
}

