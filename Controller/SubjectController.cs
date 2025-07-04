﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Constant;
using QuizHub.Models.DTO.Subject;
using QuizHub.Services.Admin_Services.Interface;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectsController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        // POST: api/Subjects
        [HttpPost]
        [Authorize("Permission.Subject.Create")]
        public async Task<IActionResult> AddSubjectAsync([FromBody] SubjectCreateDto model)
        {
            try
            {
                var result = await _subjectService.AddSubjectAsync(model);
                var url = Url.Link("SubjectDetailsRoute", result.Id);
                return Created(url, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Subjects/DeleteSubject/{id}
        [HttpDelete("{id:int}")]
        [Authorize("Permission.Subject.Delete")]
        public async Task<IActionResult> DeleteSubjectAsync(int id)
        {
            try
            {

            var result = await _subjectService.DeleteSubjectAsync(id);
            if (result)
            {
                return NoContent(); // 204 No Content
            }
            return NotFound($"Subject with ID {id} not found.");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Subjects/EditSubject/{id}
        [HttpPut("{id:int}")]
        [Authorize("Permission.Subject.Edit")]

        public async Task<IActionResult> EditSubjectAsync(int id, [FromBody] SubjectUpdateDto model)
        {
            try
            {
                var result = await _subjectService.EditSubjectAsync(id, model);
                return Ok(result); // 200 OK
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET: api/Subjects/GetAllSubjects
        [HttpGet]
        [Authorize("Permission.Subject.View")]
        public async Task<IActionResult> GetAllSubjectsAsync()
        {
            try
            {

                var userEmail = User.FindFirst(ClaimTypes.Email).Value;
                var subjects = await _subjectService.GetAllSubjectsAsync(userEmail);
                return Ok(subjects); // 200 OK
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetSubjectForTeacher")]
        [Authorize("Permission.Subject.View")]

        public async Task<IActionResult> GetSubjectsForTeacher([FromQuery] int departmentId)
        {
            var teacherEmail = User.FindFirst(ClaimTypes.Email).Value;
            var result = await _subjectService.GetSubjectForTeacher(teacherEmail, departmentId);

            if (result == null)
                return NotFound("Subjects not found or user is not a teacher.");

            return Ok(result);
        }

        // GET: api/Subjects/GetSubjectById/{id}
        [HttpGet("{id:int}", Name = "SubjectDetailsRoute")]

        [Authorize("Permission.Subject.View")]
        public async Task<IActionResult> GetSubjectByIdAsync(int id)
        {
            try
            {

                var subject = await _subjectService.GetSubjectByIdAsync(id);
                return Ok(subject);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}

