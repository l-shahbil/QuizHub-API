﻿namespace QuizHub.Models.DTO.Subject
{
    public class SubjectViewDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> LearingOutComes { get; set; }
    }
}
