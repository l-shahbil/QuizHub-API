﻿
namespace QuizHub.Models.DTO.User.Teacher
{
    public class GetTeacherDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime RegistraionDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string DepartmentName { get; set; }
    }
}
