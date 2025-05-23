using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models.DTO.User.Teacher;
using QuizHub.Services.Admin_Services.Interface;

namespace QuizHub.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _TeacherService;

        public TeacherController(ITeacherService TeacherService)
        {
            _TeacherService = TeacherService;
        }
        [HttpGet]
        [Authorize("Permission.Teacher.View")]
        public async Task<IActionResult> GetTeacher()
        {
            IEnumerable<GetTeacherDto> teacbers = await _TeacherService.GetAllTeacher();
            return Ok(teacbers);
        }
        [HttpPost]
        [Authorize("Permission.Teacher.Create")]

        public async Task<IActionResult> CreateTeacherAsync([FromBody] CreateTeacherDto model)
        {
            try
            {

            var result = await _TeacherService.CreateTeacherAsync(model);
            if (result == null)
            {
                return BadRequest("Teacher already exists or invalid data.");
            }
            string url = Url.Link("TeacherDetailsRoute", new { email = result.Email });
            return Created(url, result);
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [Authorize("Permission.Teacher.Delete")]
        [HttpDelete("{userName}")]
        public async Task<IActionResult> DeleteTeacherAsync(string userName)
        {
            var result = await _TeacherService.DeleteTeacherAsync(userName);
            if (!result)
            {
                return NotFound("Teacher not found.");
            }
            return NoContent();
        }

        [HttpPut("{userName}")]
        [Authorize("Permission.Teacher.Edit")]
        public async Task<IActionResult> EditTeacherAsync(string userName, [FromBody] UpdateTeacherDto model)
        {
            try
            {

            var result = await _TeacherService.EditTeacherAsync(userName, model);
            if (result == null)
            {
                return BadRequest("Teacher not found or password update failed.");
            }
            return Ok(result);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{userName}", Name = "TeacherDetailsRoute")]
        [Authorize("Permission.Teacher.View")]
        public async Task<IActionResult> GetTeacherByNameAsync(string userName)
        {
            var result = await _TeacherService.GetTeacherByNameAsync(userName);
            if (result == null)
            {
                return NotFound("Teacher not found.");
            }
            return Ok(result);
        }
    }
}

