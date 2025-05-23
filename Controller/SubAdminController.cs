

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Constant;
using QuizHub.Models;
using QuizHub.Models.DTO.College;
using QuizHub.Models.DTO.User.SubAdmin;
using QuizHub.Models.DTO.User.Teacher;
using QuizHub.Services.Admin_Services.Interface;
using System.Threading.Tasks;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubAdminController : ControllerBase
    {
        private readonly ISubAdminService _subAdminService;

        public SubAdminController(ISubAdminService subAdminService)
        {
            _subAdminService = subAdminService;
        }
        [HttpGet]
        [Authorize("Permission.SubAdmin.View")]
        public async Task<IActionResult> GetSubAdmin()
        {
            IEnumerable<GetSubAdminDto> subAdmins = await _subAdminService.GetAllSubAdmin();
            return Ok(subAdmins);
        }
        [HttpPost]
        [Authorize("Permission.SubAdmin.Create")]


        public async Task<IActionResult> CreateSubAdminAsync([FromBody] CreateSubAdminDto model)
        {
            try
            {
                var result = await _subAdminService.CreateSubAdminAsync(model);

                if (result == null)
                {
                    return Conflict(new { message = "SubAdmin with this email already exists." });
                }

                string url = Url.Link("SubAdminDetailsRoute", new { email = result.Email });
                return Created(url, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
           
        }
        [Authorize("Permission.SubAdmin.Delete")]
        [HttpDelete("{userName}")]
        public async Task<IActionResult> DeleteSubAdminAsync(string userName)
        {
            var result = await _subAdminService.DeleteSubAdminAsync(userName);
            if (!result)
            {
                return NotFound("SubAdmin not found.");
            }
            return NoContent();
        }

        [HttpPut("{userName}")]
        [Authorize("Permission.SubAdmin.Edit")]
        public async Task<IActionResult> EditSubAdminAsync(string userName, [FromBody] UpdateSubAdminDto model)
        {
            try
            {
                var result = await _subAdminService.EditSubAdminAsync(userName, model);

                if (result == null)
                {
                    return NotFound(new { message = "Failed to update SubAdmin." });
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{userName}", Name = "SubAdminDetailsRoute")]
        [Authorize("Permission.SubAdmin.View")]
        public async Task<IActionResult> GetSubAdminByNameAsync(string userName)
        {
            var result = await _subAdminService.GetSubAdminByNameAsync(userName);
            if (result == null)
            {
                return NotFound("SubAdmin not found.");
            }
            return Ok(result);
        }
    }
}
