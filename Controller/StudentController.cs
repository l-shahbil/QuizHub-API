using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Constant;
using QuizHub.Models.DTO.User.Student;
using QuizHub.Services.SubAdmin_Services;
using QuizHub.Services.SubAdmin_Services.Interface;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }
        [HttpPost]
        [Authorize("Permission.Student.Create")]
        public async Task<ActionResult<StudentViewDetailsDto>> CreateStudent([FromBody] StudentCreateDto model, [FromQuery] int departmentId)
        {
            try
            {
                var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var student = await _studentService.CreateStudentAsync(model, subAdminEmail, departmentId);
                return CreatedAtAction(nameof(GetStudentByName), new { departmentId = departmentId, userName = student.Email, subAdminEmail = subAdminEmail }, student);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("delete/{userName}")]
        [Authorize("Permission.Student.Delete")]
        public async Task<ActionResult> DeleteStudent(string userName, [FromQuery] int departmentId)
        {
            try
            {
                var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                bool result = await _studentService.DeleteStudentAsync(userName, subAdminEmail, departmentId);
                if (result)
                    return NoContent(); // Success: No content returned for successful deletion
                else
                    return BadRequest("Failed to delete student.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("edit/{userName}")]
        [Authorize("Permission.Student.Edit")]
        public async Task<ActionResult<StudentViewDetailsDto>> EditStudent(string userName, [FromBody] StudentEditDto model, [FromQuery] int departmentId)
        {
            try
            {
                var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                var updatedStudent = await _studentService.EditStudentAsync(userName, model, subAdminEmail, departmentId);
                if (updatedStudent == null)
                    return NotFound($"Student with email {userName} not found.");

                return Ok(updatedStudent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("all")]
        [Authorize("Permission.Student.View")]
        public async Task<ActionResult<IEnumerable<StudentViewDto>>> GetAllStudents([FromQuery] int departmentId)
        {
            try
            {
                var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                var students = await _studentService.GetAllStudent(departmentId, subAdminEmail);
                return Ok(students);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{userName}")]
        [Authorize("Permission.Student.View")]
        public async Task<ActionResult<StudentViewDetailsDto>> GetStudentByName(int departmentId, string userName)
        {
            try
            {
                var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                var student = await _studentService.GetStudentByNameAsync(departmentId, userName, subAdminEmail);
                return Ok(student);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("studentsCounts")]
        [Authorize]
        public async Task<ActionResult> GetStudentsCounts()
        {
            int studentsCount = await _studentService.GetStudentsCounts();
            return Ok(studentsCount);
        }
    }
}
