using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models;
using QuizHub.Models.DTO.Class;
using QuizHub.Services.SubAdmin_Services.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly UserManager<AppUser> _userManger;

        public ClassController(IClassService classService,UserManager<AppUser> userManger)
        {
            _classService = classService;
            _userManger = userManger;
        }

        // POST: api/Class/addClass
        [HttpPost("addClass")]
        [Authorize("Permission.Class.Create")]
        public async Task<ActionResult<ClassViewDto>> AddClassAsync(ClassCreateDto model, int departmentId,int subjectId, string teacherEmail)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var newClass = await _classService.AddClasssAsync(model,userEmail,departmentId, subjectId, teacherEmail);

                return Created("Created Successfuly",newClass);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize("Permission.Class.Edit")]
        public async Task<ActionResult<ClassViewDto>> EditClassAsync(int id, ClassUpdateDto model)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var updatedClass = await _classService.EditClasssAsync(model,id, userEmail);
                return Ok(updatedClass);
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize("Permission.Class.Delete")]
        public async Task<ActionResult> DeleteClassAsync(int id)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var result = await _classService.DeleteClasssAsync(id,userEmail);
                if (!result)
                {
                    return NotFound(new { message = "Class not found." });
                }
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/Class/forUser
        [HttpGet]
        [Authorize("Permission.Class.View")]
        public async Task<ActionResult<List<ClassViewDto>>> GetAllClassesForUserAsync()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email).Value;
            var user =await _userManger.FindByEmailAsync(userEmail);
            var userType = _userManger.GetRolesAsync(user).Result.FirstOrDefault();

            if (string.IsNullOrEmpty(userType))
            {
                return BadRequest(new { message = "User type is not defined." });
            }

            try
            {
                List<ClassViewDto> classes;
                switch (userType.ToLower())
                {
                    case "subadmin":
                        var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                        if (string.IsNullOrEmpty(subAdminEmail))
                        {
                            return Unauthorized(new { message = "SubAdmin Email is required." });
                        }
                        classes = await _classService.GetAllClassesForSubAdminAsync(1, subAdminEmail);
                        break;

                    case "teacher":
                        var teacherUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (string.IsNullOrEmpty(teacherUserId))
                        {
                            return Unauthorized(new { message = "Teacher UserId is required." });
                        }
                        classes = await _classService.GetAllClassesForTeacherAsync(teacherUserId);
                        break;

                    case "student":
                        var studentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (string.IsNullOrEmpty(studentUserId))
                        {
                            return Unauthorized(new { message = "Student UserId is required." });
                        }
                        classes = await _classService.GetAllClassesForStudentAsync(studentUserId);
                        break;

                    default:
                        return Unauthorized(new { message = "User role is not recognized." });
                }

                return Ok(classes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
