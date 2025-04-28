using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models;
using QuizHub.Models.DTO.Question;
using QuizHub.Services.Shared_Services.Interface;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly UserManager<AppUser> _userManager;

        public QuestionController(IQuestionService questionService, UserManager<AppUser> userManager)
        {
            _questionService = questionService;
            _userManager = userManager;
        }

        [HttpPost("{departmentId:int}/{learningOutComesId:int}")]
        [Authorize("Permission.Question.Create")]
        public async Task<IActionResult> AddQuestion([FromBody] QuestionCreateDto model, int departmentId, int learningOutComesId, [FromQuery] int? classId)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (userEmail == null) return Unauthorized("User email not found.");

                var result = await _questionService.AddQuestionAsync(userEmail, departmentId, learningOutComesId, model, classId);
                return Created("", result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{questionId:int}")]
        [Authorize("Permission.Question.Delete")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (userEmail == null) return Unauthorized("User email not found.");

                var result = await _questionService.DeleteQuestionAsync(userEmail, questionId);
                return result ? Ok("Question deleted successfully.") : BadRequest("Failed to delete question.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{questionId:int}")]
        [Authorize("Permission.Question.Edit")]
        public async Task<IActionResult> EditQuestion(int questionId, [FromBody] QuestionUpdateDto model)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (userEmail == null) return Unauthorized("User email not found.");

                var result = await _questionService.EditQuestionAsync(userEmail, questionId, model);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        [Authorize("Permission.Question.View")]
        public async Task<IActionResult> GetAllQuestions([FromQuery] int subjectId)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (userEmail == null) return Unauthorized("User email not found.");

                var result = await _questionService.GetAllQuestion(userEmail, subjectId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize("Permission.Question.View")]
        [HttpGet("{questionId:int}")]
        public async Task<IActionResult> getQuestion([FromRoute]int questionId)
        {
            try
            {
                var result = await _questionService.GetQuestionById(questionId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}