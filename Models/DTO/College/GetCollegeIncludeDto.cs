using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.College
{
    public class GetCollegeIncludeDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> departmentName { get; set; }

    }
}
