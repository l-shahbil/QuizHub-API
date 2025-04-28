using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models.DTO.Exam;
using QuizHub.Services.Shared_Services.Interface;
using System.Security.Claims;

namespace QuizHub.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamsController(IExamService examService)
        {
            _examService = examService;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateExamAsync([FromQuery] int departmentId, [FromBody] ExamCreateDto model)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null) return Unauthorized("User email not found.");

            try
            {
                var examViewDto = await _examService.CreateExamAsync(userEmail, departmentId, model);

                if (examViewDto == null)
                {
                    return BadRequest("Failed to create exam.");
                }

                return Ok(examViewDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception (optional for now)
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpPost("publish")]
        public async Task<IActionResult> PublishExam([FromQuery] string examId, [FromQuery] int classId, [FromBody] ExamPublishDto model)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (userEmail == null) return Unauthorized("User email not found.");


                bool result = await _examService.ExamPuplish(userEmail, examId, classId, model);

                if (result)
                    return Created();

                return BadRequest("Failed to publish exam.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred. " + ex.Message);
            }
        }
        [HttpDelete("cancel-publication")]
        public async Task<IActionResult> CancelExamPublication([FromQuery] int classExamId)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (userEmail == null) return Unauthorized("User email not found.");


                bool result = await _examService.CancelExamPublication(userEmail, classExamId);

                if (result)
                    return NoContent();

                return BadRequest("Failed to cancel exam publication.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred. " + ex.Message);
            }
        }
    }
}
