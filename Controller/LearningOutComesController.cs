using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Constant;
using QuizHub.Models.DTO.LearingOutComes;
using QuizHub.Services.Admin_Services.Interface;
using System.Threading.Tasks;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningOutcomesController : ControllerBase
    {
        private readonly ILearningOutComesService _learningOutComesService;

        public LearningOutcomesController(ILearningOutComesService learningOutComesService)
        {
            _learningOutComesService = learningOutComesService;
        }


        [HttpPost("{subjectId:int}")]
        [Authorize("Permission.LearingOutcomes.Create")]
        public async Task<IActionResult> AddLearningOutComesAsync(int subjectId, [FromBody] LearningOutComesCreateDto model)
        {
            try
            {
                var result = await _learningOutComesService.AddLearningOutComesAsync(subjectId, model);
                var url = Url.Link("LearningOutComesDetailsRoute", result.Id);
                return Created(url, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize("Permission.LearingOutcomes.Delete")]
        public async Task<IActionResult> DeleteLearningOutComesAsync(int id)
        {
            var result = await _learningOutComesService.DeleteLearningOutComesAsync(id);
            if (result)
            {
                return NoContent(); // 204 No Content
            }
            return NotFound($"Learning Outcome with ID {id} not found.");
        }

        [HttpPut("{id:int}")]
        [Authorize("Permission.LearingOutcomes.Edit")]
        public async Task<IActionResult> EditLearningOutComesAsync(int id, [FromBody] LearningOutComesUpdateDto model)
        {
            try
            {
                var result = await _learningOutComesService.EditLearningOutComesAsync(id, model);
                return Ok(result); // 200 OK
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{subjectId:int}")]
        [Authorize("Permission.LearingOutcomes.View")]
        public async Task<IActionResult> GetAllLearningOutComesAsync(int subjectId)
        {
            try
            {
                var outcomes = await _learningOutComesService.GetAllLearningOutComesAsync(subjectId);
                return Ok(outcomes); // 200 OK
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpGet("getById/{id:int}", Name = "LearningOutComesDetailsRoute")]
        [Authorize("Permission.LearingOutcomes.View")]
        public async Task<IActionResult> GetLearningOutComesById(int id)
        {
            try
            {
                var result = await _learningOutComesService.GetLearningOutComesByIdAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Return a 404 Not Found if the subject is not found.
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error for any unexpected exceptions.
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
