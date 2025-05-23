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
        public async Task<ActionResult<List<ClassViewDto>>> GetAllClassesForUserAsync([FromQuery]int departmentId = 000000000)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManger.FindByEmailAsync(userEmail);
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
                      
                        classes = await _classService.GetAllClassesForSubAdminAsync(departmentId, subAdminEmail);
                        break;

                    case "teacher":
                        var teacherUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (string.IsNullOrEmpty(teacherUserId))
                        {
                            return Unauthorized(new { message = "Teacher UserId is required." });
                        }
                        classes = await _classService.GetAllClassesForTeacherAsync(departmentId,teacherUserId);
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

        [HttpPost("{departmentId:int}/{classId:int}/students")]
        [Authorize("Permission.Student.Create")]
            public async Task<IActionResult> AddStudentToClass(int departmentId, int classId, string studentEmail)
            {
            try
            {
                var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (subAdminEmail == null)
                {
                    return Unauthorized("Invalid user token.");
                }
                await _classService.AddStudentToClass(departmentId, subAdminEmail, classId, studentEmail);
                return Ok("Student added successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
            }
        [HttpDelete("{departmentId:int}/{classId:int}/students/{studentEmail}")]
        [Authorize("Permission.Student.Delete")]
            public async Task<IActionResult> DeleteStudentFromClass(int departmentId,  int classId, string studentEmail)
            {
                try
                {
                var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (subAdminEmail == null)
                {
                    return Unauthorized("Invalid user token.");
                }
                await _classService.DeleteStudentFromClass(departmentId, subAdminEmail, classId, studentEmail);
                    return Ok("Student removed successfully.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    return StatusCode(403,ex.Message);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{departmentId:int}/{classId:int}/batches/{batchId:int}")]
        [Authorize("Permission.Student.Create")]
            public async Task<IActionResult> AddBatchToClass(int departmentId,  int classId, int batchId)
            {
                try
                {
                var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (subAdminEmail == null)
                {
                    return Unauthorized("Invalid user token.");
                }
                await _classService.AddBatchToClass(departmentId, subAdminEmail, classId, batchId);
                    return Ok("Batch added successfully.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    return StatusCode(403,ex.Message);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "An unexpected error occurred.");
                }
            }
        [HttpGet("{departmentId:int}/{classId:int}/students")]
        [Authorize("Permission.Student.View")]
        public async Task<IActionResult> GetAllStudentInClass(int departmentId, int classId)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var students = await _classService.GetAllStudentInClass(departmentId, userEmail, classId);
                return Ok(students);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
    
    
}
