using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Constant;
using QuizHub.Models.DTO.Batch;
using QuizHub.Services.SubAdmin_Services.Interface;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BatchController : ControllerBase
    {
        private readonly IBatchService _batchService;

        public BatchController(IBatchService batchService)
        {
            _batchService = batchService;
        }
        [HttpPost]
        [Authorize("Permission.Batch.Create")]
        public async Task<IActionResult> AddBatch([FromBody] BatchCreateDto model, [FromQuery] int departmentId)
        {
            try
            {

     
            var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (subAdminEmail == null)
            {
                return Unauthorized("Invalid user token.");
            }

            var batch = await _batchService.AddBatchAsync(model, subAdminEmail, departmentId);
                var url = Url.Link("GetBatchDetails",batch.Id);
                return Created(url,batch);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
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

        [HttpDelete("{id}")]
        [Authorize("Permission.Batch.Delete")]

        public async Task<IActionResult> DeleteBatch(int id)
        {
            try
            {

            var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (subAdminEmail == null)
            {
                return Unauthorized("Invalid user token.");
            }

            var result = await _batchService.DeleteBatchAsync(id, subAdminEmail);
            return result ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
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

        [HttpPut("{id}")]
        [Authorize("Permission.Batch.Edit")]

        public async Task<IActionResult> EditBatch(int id, [FromBody] BatchEditDto model)
        {
            try
            {

            var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (subAdminEmail == null)
            {
                return Unauthorized("Invalid user token.");
            }

            var batch = await _batchService.EditBatchAsync(model, id, subAdminEmail);
            return Ok(batch);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
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

        [HttpGet("department/{departmentId:int}")]
        [Authorize("Permission.Batch.View")]

        public async Task<IActionResult> GetAllBatches(int departmentId)
        {
            try
            {

                var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (subAdminEmail == null)
                {
                    return Unauthorized("Invalid user token.");
                }

                var batches = await _batchService.GetAllBathcesAsync(departmentId, subAdminEmail);
                return Ok(batches);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
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

        [HttpGet("{id:int}",Name ="GetBatchDetails")]
        [Authorize("Permission.Batch.View")]
        public async Task<IActionResult> GetBatchById(int id)
        {
            try
            {

            var subAdminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (subAdminEmail == null)
            {
                return Unauthorized("Invalid user token.");
            }

            var batch = await _batchService.GetBatchById(id, subAdminEmail);
            return Ok(batch);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
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