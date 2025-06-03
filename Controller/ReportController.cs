using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Services.SubAdmin_Services.Interface;

namespace QuizHub.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [Authorize("Permission.Exam.Display Report")]
        [HttpGet("exam-report")]
        public async Task<IActionResult> GetExamReport([FromQuery] int classId, [FromQuery] string examId)
        {
            try
            {
                // Get the current user's email from the claims
                var userEmail = User.Identity?.Name;

                var report = await _reportService.getReportExam(userEmail, classId, examId);

                return Ok(report);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Something went wrong: {ex.Message}");
            }
        }
    }
}
