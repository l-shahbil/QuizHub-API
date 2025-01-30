using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using QuizHub.Models;
using QuizHub.Models.DTO.Authentication;
using QuizHub.Services.Authentication.Login_And_Sing_Up.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuizHub.Services.Authentication.Login_And_Sing_Up
{
    public class AuthenService : IAuthenService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthenService(UserManager<AppUser> userManager,RoleManager<IdentityRole>roleManger, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManger;

            _config = config;
        }
        public async Task<JwtResponse> authenticationAsync(LoginDto userDto)
        {
            AppUser user = await _userManager.FindByNameAsync(userDto.userName);
            if (user == null)
            {
                return null;
            }
            bool found = await _userManager.CheckPasswordAsync(user, userDto.password);
            if (!found)
            {
                return null;
            }

            //Create Claims
            var Myclaims = new List<Claim>();
            Myclaims.Add(new Claim(ClaimTypes.Name, userDto.userName));
            Myclaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            Myclaims.Add(new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            //get role and permission
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach(var claim in roleClaims)
                    {
                        Myclaims.Add(new Claim("Permission", claim.Value));
                    }
                }
            }


            var permissions = await _userManager.GetClaimsAsync(user);
            foreach (var per in permissions)
            {
                Myclaims.Add(new Claim("Permission", per.Value));
            }

            //create SingingCredentials
            SecurityKey myKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));
            SigningCredentials singingCred = new SigningCredentials(myKey, SecurityAlgorithms.HmacSha256);

            //Init Token
            JwtSecurityToken myToken = new JwtSecurityToken(
                issuer: _config["JWT:validIssure"],
                audience: _config["JWT:validAudience"],
                expires: DateTime.Now.AddHours(1),
                claims: Myclaims,
                signingCredentials: singingCred
                );
            return new JwtResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(myToken),
                Expiration = myToken.ValidTo
            };

        }
    }

}
