using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models;
using QuizHub.Models.DTO.College;
using QuizHub.Services.Admin_Services.Interface;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]

    [ApiController]
    public class CollegeController : ControllerBase
    {
        private readonly ICollegeService _collegeService;
        public CollegeController(ICollegeService collegeService)
        {
            _collegeService = collegeService;
        }

        [HttpGet]
        [Authorize("Permission.College.View")]
        public async Task<IActionResult> GetCollege()
        
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            List<GetCollegeDto> colleges = await _collegeService.GetAllCollegesAsync(userEmail);
            return Ok(colleges);
        }

        [HttpGet("{id}:int",Name ="CollegeDetailsRoute")]
        [Authorize("Permission.College.View")]
        public async Task<IActionResult> GetCollegeById([FromRoute] int id)
        {
            
            GetCollegeIncludeDto college = await _collegeService.GetCollegeByIdAsync(id);
            if (college != null)
                return Ok(college);
            return NotFound();
        }

        [HttpPost]
        [Authorize("Permission.College.Create")]
        public async Task<IActionResult> AddCollege(CreateCollegeDto model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            var college = await _collegeService.AddCollegeAsync(model);
            if (college == null)
            {
                return Conflict("College already exists.");
            }
            string url = Url.Link("CollegeDetailsRoute", new { id = college.Id });
            return Created(url,college);
            

        }
        [HttpPut("{id}:int")]
        [Authorize("Permission.College.Edit")]
        public async Task<IActionResult> EditCollege([FromRoute] int id, [FromBody] UpdateCollegeDto model)
        {
            try
            {
                var updatedCollege = await _collegeService.EditCollegeAsync(id, model);

                if (updatedCollege == null)
                    return NotFound(new { message = "College not found." });

                return Ok(updatedCollege);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }

        }

        [HttpDelete("{id}:int")]
        [Authorize("Permission.College.Delete")]
        public async Task<IActionResult> DeleteCollege([FromRoute] int id)
        {
            var result = await _collegeService.DeleteCollegeAsync(id);
            if (!result)
                return NotFound();


            return NoContent();

        }
    }
}
