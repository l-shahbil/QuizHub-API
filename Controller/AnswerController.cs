using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Constant;
using QuizHub.Models.DTO.Answer;
using QuizHub.Services.Shared_Services.Interface;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly IAnswerService _answerService;

        public AnswerController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        [HttpPost]
        [Authorize("Permission.Question.Create")]
        public async Task<IActionResult> AddAnswer([FromQuery] int questionId, [FromBody] AnswerCreateDto model)
        {
            try
            {
                string userEmail = User.FindFirst(ClaimTypes.Email).Value;
                var answer = await _answerService.addAnswerAsync(userEmail, questionId, model);
                return CreatedAtAction(nameof(AddAnswer), new { id = answer.Id }, answer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }
        [Authorize("Permission.Question.Delete")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAnswer([FromQuery] int questionId, [FromQuery] int answerId)
        {
            try
            {
                string userEmail = User.FindFirst(ClaimTypes.Email).Value;
                bool result = await _answerService.DeleteAnswerAsync(userEmail, questionId, answerId);
                if (!result)
                {
                    return BadRequest(new { message = "Unable to delete the answer." });
                }
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }

        [HttpPut]
        [Authorize("Permission.Question.Edit")]
        public async Task<IActionResult> EditAnswer([FromQuery] int questionId, [FromQuery] int answerId, [FromBody] AnswerEditDto model)
        {
            try
            {
                string userEmail = User.FindFirst(ClaimTypes.Email).Value;
                var updatedAnswer = await _answerService.EditAnswerAsync(userEmail, questionId, answerId, model);
                return Ok(updatedAnswer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }
    }
}
