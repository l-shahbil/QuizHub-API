using Microsoft.AspNetCore.Identity;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.LearingOutComes;
using QuizHub.Models.DTO.Subject;
using QuizHub.Services.Admin_Services.Interface;

namespace QuizHub.Services.Admin_Services
{
    public class SubjectService : ISubjectService
    {
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly UserManager<AppUser> _userManger;

        public SubjectService(IRepository<Subject> subjectRepo,IRepository<Department> departmentRepo,UserManager<AppUser> userManger)
        {
            _subjectRepo = subjectRepo;
            _departmentRepo = departmentRepo;
            _userManger = userManger;
        }
        public async Task<SubjectViewDto> AddSubjectAsync(SubjectCreateDto model)
        {
           var existSubject = await _subjectRepo.SelecteOne(sub=> sub.Name == model.Name);
            if (existSubject != null) 
            {
                throw new ArgumentException("A Subject with the same name already exists.");
            }



            var newSubject = new Subject()
            {
                Name = model.Name,
                Description = model.Description
            };
            await _subjectRepo.AddAsyncEntity(newSubject);
            return new SubjectViewDto
            {
                Id = newSubject.Id,
                Name = model.Name,
                Description = model.Description
            };
        }

        public async Task<bool> DeleteSubjectAsync(int id)
        {
            Subject subject = await _subjectRepo.GetByIdAsync(id);
            if (subject == null)
            {
                return false;
            }

            _subjectRepo.DeleteEntity(subject);
            return true;
        }

        public async Task<SubjectViewDto> EditSubjectAsync(int id, SubjectUpdateDto model)
        {
            var subject = await _subjectRepo.GetByIdAsync(id)
             ?? throw new KeyNotFoundException($"Subject with ID {id} not found.");
            var existSubject = await _subjectRepo.SelecteOne(sub => sub.Name == model.Name);
            if (existSubject != null)
            {
                throw new ArgumentException("A Subject with the same name already exists.");
            }
            subject.Name =string.IsNullOrWhiteSpace(model.Name) ? subject.Name:model.Name;
            subject.Description = string.IsNullOrWhiteSpace(model.Description) ? subject.Description:model.Description;

            _subjectRepo.UpdateEntity(subject);
            return new SubjectViewDto
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description
            };
        }

        public async Task<List<SubjectViewDto>> GetAllSubjectsAsync(string userEmail)
        {
           
                var user = await _userManger.FindByEmailAsync(userEmail);
                var roles = await _userManger.GetRolesAsync(user);
                List<SubjectViewDto> subjects = new List<SubjectViewDto>();

                if (roles.Contains("Admin"))
                {
                subjects = _subjectRepo.GetAllAsync().Result.Select(s => new SubjectViewDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                }).ToList();

                    return subjects;
                }
                else if (roles.Contains("SubAdmin"))
                {
                    var departments = await _departmentRepo.GetAllIncludeAsync("Subjects");

                    subjects = departments
                        .Where(d => d.subAdminId == user.Id)
                        .SelectMany(d => d.Subjects)
                        .Select(s => new SubjectViewDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Description = s.Description
                        }).ToList();
                }

                return subjects;
            
        }


        public async Task<SubjectViewDetailsDto> GetSubjectByIdAsync(int id,string userEmail)
        {
            var subject = await _subjectRepo.GetIncludeById(id, "LearingOutcomes");
            if (subject == null)
            {
                throw new ArgumentException("A Subject not found.");
            }
            var user = await _userManger.FindByEmailAsync(userEmail);
            var roles = await _userManger.GetRolesAsync(user);

            if (roles.Contains("Admin"))
            {
                return new SubjectViewDetailsDto()
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    Description = subject.Description,
                    LearingOutComes = subject.LearingOutcomes.Select(l => $"ID: {l.Id}, Title: {l.Title}, Description: {l.Description}")
                    .ToList()

                };
            }
            //else if (roles.Contains("subAdmin"))
            //{
            //    var department

            //    var departments = await _departmentRepo.GetAllIncludeAsync("Subjects");
            //   var IsSubjectAssignedToDepartment
            //}

            return new SubjectViewDetailsDto();

        }
    
    }
}
