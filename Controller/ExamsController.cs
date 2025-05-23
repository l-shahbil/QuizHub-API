using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Constant;
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
        [Authorize("Permission.Exam.Create")]
        
        public async Task<IActionResult> CreateExamAsync([FromQuery] int departmentId, [FromBody] ExamCreateDto model)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email).Value;
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
                return StatusCode(403,ex.Message);
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
        [HttpPut("Edit")]
        public async Task<IActionResult> UpdateExamAsync([FromQuery] int departmentId,[FromQuery] string examId,[FromBody] ExamUpdateDto model)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var result = await _examService.UpdateExamAsynct(userEmail, departmentId, examId, model);
                if (result == null)
                    return Forbid();

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("publish")]
        [Authorize("Permission.Exam.Puplish Exam")]

        public async Task<IActionResult> PublishExam([FromQuery] string examId, [FromQuery] int classId, [FromBody] ExamPublishDto model)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email).Value;
                if (userEmail == null) return Unauthorized("User email not found.");


                bool result = await _examService.ExamPuplish(userEmail, examId, classId, model);

                if (result)
                    return Created();

                return BadRequest("Failed to publish exam.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403,ex.Message);
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
        [HttpPost("enable-show-result")]
        [Authorize("Permission.Exam.Puplish Exam")]

        public async Task<IActionResult> EnableShowResult([FromQuery] int classId, [FromQuery] string examId)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email).Value;
                if (userEmail == null) return Unauthorized("User email not found.");
                bool result = await _examService.enableShowResult(userEmail, classId, examId);
                if (result)
                    return Ok(new { message = "Show result enabled successfully." });
                else
                    return StatusCode(403,"You are not authorized to enable showing results.");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong.", details = ex.Message });
            }
        }
        [HttpDelete("cancel-publication")]
        [Authorize("Permission.Exam.Delete")]

        public async Task<IActionResult> CancelExamPublication([FromQuery] int classId,[FromQuery] string examId)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email).Value;
                if (userEmail == null) return Unauthorized("User email not found.");


                bool result = await _examService.CancelExamPublication(userEmail, classId,examId);

                if (result)
                    return NoContent();

                return BadRequest("Failed to cancel exam publication.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403,ex.Message);
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
        [HttpDelete("{examId}")]
        [Authorize("Permission.Exam.Delete")]

        public async Task<IActionResult> DeleteExam(string examId, int departmentId)
        {
            var userEmail = User.Identity?.Name;

            try
            {
                bool result = await _examService.DeleteExamAsync(userEmail, examId, departmentId);
                if (!result)
                    return Forbid("You are not authorized to delete this exam.");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("subject/{subjectId}")]
        [Authorize("Permission.Exam.View")]
        public async Task<IActionResult> GetAllExams(int subjectId, int departmentId)
        {
            var userEmail = User.Identity?.Name;

            try
            {
                var exams = await _examService.GetAllExams(userEmail, subjectId, departmentId);
                return Ok(exams);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong.", details = ex.Message });
            }
        }

        [HttpGet("{examId}")]
        [Authorize("Permission.Exam.View")]
        public async Task<IActionResult> GetExamById(string examId, int departmentId)
        {
            var userEmail = User.Identity?.Name;

            try
            {
                var exam = await _examService.GetExamById(userEmail, examId, departmentId);
                if (exam == null)
                    return Forbid("You are not authorized to view this exam.");

                return Ok(exam);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("available")]
        [Authorize("Permission.Exam.View Available")]
        public async Task<IActionResult> GetAvailableExams(int classId)
        {
            var userEmail = User.Identity?.Name;

            try
            {
                var exams = await _examService.GetExamsAvalibie(userEmail, classId);
                return Ok(exams);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("published-In-Class")]
        [Authorize("Permission.Exam.Puplish Exam")]
        public async Task<IActionResult> GetExamPublishedInClass(int classId)
        {
            var userEmail = User.Identity?.Name;

            try
            {
                var exams = await _examService.getExamPublishedInClass(userEmail, classId);
                return Ok(exams);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("take")]
        [Authorize("Permission.Exam.Take")]
        public async Task<IActionResult> TakeExam([FromQuery] int classId, [FromQuery] string examId)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var result = await _examService.ExamTake(userEmail, classId, examId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // غير متوقع
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
        [HttpPost("submit")]
        [Authorize("Permission.Exam.Submit")]
        public async Task<ActionResult<ExamResultViewDto>> SubmitExam([FromBody] ExamStudentSubmitDto model)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var result = await _examService.ExamSubmission(userEmail, model);

                if (result == null)
                    return NoContent(); 

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("getResult")]
        [Authorize("Permission.Exam.Result")]
        public async Task<IActionResult> ViewExamResult([FromQuery] string studentEmail, [FromQuery] int classId, [FromQuery] string examId)
        {
            try
            {
                var result = await _examService.ViewExamResult(studentEmail, classId, examId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("previousExams")]
        [Authorize("Permission.Exam.View Previous")]
        public async Task<IActionResult> GetExamPrevious([FromQuery] int classId)
        {
            try
            {
                var studentEmail = User.Identity?.Name;

                var result = await _examService.GetExamPrevious(studentEmail, classId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("attendence")]
        [Authorize("Permission.Exam.Display Attendance")]

        public async Task<IActionResult> DisplayExamAttendenceAndResults( [FromQuery] int classId, [FromQuery] string examId)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var result = await _examService.DisplayExamAttendenceAndResults(userEmail, classId, examId);
                if (result == null)
                    return Forbid();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("generatePractices")]
        [Authorize("Permission.Exam.Practices")]
        public async Task<IActionResult> GenerateExamPractice(
       [FromQuery] int classId,
       [FromQuery] List<int> learningOutcomeIds,
       [FromQuery] int questionCount)
        {
            try
            {
                var studentEmail = User.FindFirst(ClaimTypes.Email).Value;
                var exam = await _examService.GetExamPractices(studentEmail, classId, learningOutcomeIds, questionCount);
                return Ok(exam);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("submitPractices")]
        [Authorize("Permission.Exam.Submit")]
        public async Task<IActionResult> SubmitPracticeExam(
            [FromBody] ExamStudentSubmitDto model)
        {
            try
            {
                var studentEmail = User.FindFirst(ClaimTypes.Email).Value;
                var result = await _examService.ExamSubmissionPractices(studentEmail, model);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
