using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Subject;
using QuizHub.Services.Admin_Services.Interface;

namespace QuizHub.Services.Admin_Services
{
    public class SubjectService : ISubjectService
    {
        private readonly IRepository<Subject> _subjectRepo;

        public SubjectService(IRepository<Subject> subjectRepo)
        {
            _subjectRepo = subjectRepo;
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

        public async Task<List<SubjectViewDto>> GetAllSubjectsAsync()
        {

            List<SubjectViewDto> subjects = _subjectRepo.GetAllIncludeAsync("LearingOutcomes").Result.Select(s => new SubjectViewDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LearingOutComes = s.LearingOutcomes.Select(l => l.Title).ToList()
                
            }).ToList();
            return subjects;
        }

        public async Task<SubjectViewDto> GetSubjectByIdAsync(int id)
        {
            var subject = await _subjectRepo.GetIncludeById(id, "LearingOutcomes");
            if (subject == null)
            {
                throw new ArgumentException("A Subject not found.");
            }
            return new SubjectViewDto { 
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description,
                LearingOutComes = subject.LearingOutcomes
            .Select(l => $"ID: {l.Id}, Title: {l.Title}, Description: {l.Description}")
            .ToList()
            };
        }
    }
}
