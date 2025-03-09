using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models;
using QuizHub.Models.DTO.Department;
using QuizHub.Models.DTO.User.Teacher;
using QuizHub.Services.Admin_Services.Interface;

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
        [HttpPost("{departmentId}/teachers/{teacherEmail}")]
        [Authorize("Permission.Teacher.Add To Department")]
        public async Task<IActionResult> AddTeacherToDepartment(int departmentId, string teacherEmail)
        {
            try
            {
                var result = await _departmentService.AddTeacherToDepartmentAsync(departmentId, teacherEmail);

                return StatusCode(StatusCodes.Status201Created,"Teacher added to department successfully.");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{departmentId}/teachers/{teacherEmail}")]
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
            var teachers = await _departmentService.GetAllTeachersInDepartmentAsync(departmentId);
            if (teachers == null || !teachers.Any())
            {
                return NotFound("No teachers found for this department.");
            }
            return Ok(teachers);
        }


        [HttpPost("{departmentId:int}/subjects/{subjectId:int}")]
        [Authorize("Permission.Subject.Add To Department")]
        public async Task<IActionResult> AddSubjectToDepartment(int departmentId, int subjectId)
        {
            try
            {

                var result = await _departmentService.AddSubjectToDepartmentAsync(departmentId, subjectId);
                return StatusCode(StatusCodes.Status201Created, "Subject added to department successfully.");
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{departmentId:int}/subjects/{subjectId:int}")]
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

