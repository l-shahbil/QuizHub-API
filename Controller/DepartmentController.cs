using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models;
using QuizHub.Models.DTO.Department;
using QuizHub.Models.DTO.User.Teacher;
using QuizHub.Services.Admin_Services.Interface;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [Authorize("Permission.Department.View")]
        public async Task<IActionResult> Getdepartment()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();

            
            return Ok(departments);
        }

        [HttpGet("{id}:int", Name = "DepartmentDetailsRoute")]
        [Authorize("Permission.Department.View")]
        public async Task<IActionResult> GetdepartmentById([FromRoute] int id)
        {

            DepartmentViewDto department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department != null)
                return Ok(department);
            return NotFound();
        }
        [HttpGet("ByCollege/{collegeId}")]
        [Authorize("Permission.Department.View")]
        public async Task<ActionResult<List<DepartmentViewDto>>> GetDepartmentsByCollegeId(int collegeId)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email).Value;
                var departments = await _departmentService.getDepartmentByCollegeId(userEmail, collegeId);

                return Ok(departments);
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


        [HttpPost]
        [Authorize("Permission.Department.Create")]
        public async Task<IActionResult> Adddepartment(DepartmentCreateDto model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var department = await _departmentService.AddDepartmentAsync(model);
                if (department == null)
                {
                    return Conflict("department already exists.");
                }
                string url = Url.Link("departmentDetailsRoute", new { id = department.Id });
                return Created(url, department);
            }
            catch(Exception ex) 
            {
                return BadRequest(ex.Message);
            }


        }
        [HttpPut("{id}:int")]
        [Authorize("Permission.Department.Edit")]
        public async Task<IActionResult> Editdepartment([FromRoute] int id, [FromBody] DepartmentUpdateDto model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updateddepartment = await _departmentService.EditDepartmentAsync(id, model);
                return NoContent();
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);

            }

        }

        [HttpDelete("{id}:int")]
        [Authorize("Permission.Department.Delete")]
        public async Task<IActionResult> Deletedepartment([FromRoute] int id)
        {
            var result = await _departmentService.DeleteDepartmentAsync(id);
            if (!result)
                return NotFound();


            return NoContent();

        }
        [HttpPost("{departmentId}/teachers")]
        [Authorize("Permission.Teacher.Add To Department")]
        public async Task<IActionResult> AddTeacherToDepartment(int departmentId,List<string> teachersEmails)
        {
            try
            {
                var result = await _departmentService.AddTeacherToDepartmentAsync(departmentId, teachersEmails);

                return StatusCode(StatusCodes.Status201Created,"Teacher added to department successfully.");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{departmentId}/teachers")]
        [Authorize("Permission.Teacher.Delete From Department")]
        public async Task<IActionResult> DeleteTeacherFromDepartment(int departmentId, string teacherEmail)
        {
            try
            {
                var result = await _departmentService.DeleteTeacherFromDepartmentAsync(departmentId, teacherEmail);

                return StatusCode(StatusCodes.Status200OK, "Teacher removed from department successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{departmentId}/teachers")]
        [Authorize("Permission.Teacher.View")]
        public async Task<IActionResult> GetAllTeachersInDepartment(int departmentId)
        {
            try
            {
                var teachers = await _departmentService.GetAllTeachersInDepartmentAsync(departmentId);
                return Ok(teachers);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                return Forbid(ua.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpPost("{departmentId:int}/subjects")]
        [Authorize("Permission.Subject.Add To Department")]
        public async Task<IActionResult> AddSubjectToDepartment(int departmentId, List<int> subjectIds)
        {
            try
            {

                var result = await _departmentService.AddSubjectToDepartmentAsync(departmentId, subjectIds);
                return StatusCode(StatusCodes.Status201Created, "Subject added to department successfully.");
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{departmentId:int}/subjects")]
        [Authorize("Permission.Subject.Delete From Department")]

        public async Task<IActionResult> DeleteSubjectFromDepartment(int departmentId, int subjectId)
        {
            try
            {

                var result = await _departmentService.DeleteSubjectFromDepartmentAsync(departmentId, subjectId);
                return StatusCode(StatusCodes.Status200OK, "Subject removed from department successfully.");
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{departmentId}/subjects")]
        [Authorize("Permission.Subject.View")]

        public async Task<IActionResult> GetAllSubjectsInDepartment(int departmentId)
        {
            var subjects = await _departmentService.GetAllSubjectsInDepartmentAsync(departmentId);
            return Ok(subjects);
        }
    }


}

