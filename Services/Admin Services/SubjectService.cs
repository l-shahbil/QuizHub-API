using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.LearingOutComes;
using QuizHub.Models.DTO.Subject;
using QuizHub.Services.Admin_Services.Interface;
using QuizHub.Utils.Interface;
using System.Security.Cryptography;

namespace QuizHub.Services.Admin_Services
{
    public class SubjectService : ISubjectService
    {
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly UserManager<AppUser> _userManger;
        private readonly IRepository<Class> _classRepo;
        private readonly IDeleteService _deleteService;
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<StudentAnswers> _studentAnswers;
        private readonly IRepository<Answer> _answerRepo;
        private readonly IRepository<LearningOutcomes> _learningOutComesRepo;

        public SubjectService(IRepository<Subject> subjectRepo,IRepository<Department> departmentRepo,UserManager<AppUser> userManger
            ,IRepository<Class> classRepo,IDeleteService deleteService,IRepository<Question>questionRepo,
            IRepository<StudentAnswers>studentAnswers,IRepository<Answer> answerRepo,IRepository<LearningOutcomes>learningOutComesRepo)
        {
            _subjectRepo = subjectRepo;
            _departmentRepo = departmentRepo;
            _userManger = userManger;
            _classRepo = classRepo;
            _deleteService = deleteService;
            _questionRepo = questionRepo;
            _studentAnswers = studentAnswers;
            _answerRepo = answerRepo;
            _learningOutComesRepo = learningOutComesRepo;
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


            Subject subj =await _subjectRepo.GetFirstOrDefaultAsync(
                filter:sub=> sub.Id ==subject.Id,
                include:query=> query.Include(s=> s.LearingOutcomes).ThenInclude(lrn=> lrn.Questions).ThenInclude(q=> q.Answers)
                                .Include(s => s.LearingOutcomes).ThenInclude(lrn => lrn.Questions).ThenInclude(q => q.StudentAnswers)
                                .Include(s=> s.Exams)
                                .Include(s=> s.Classes),
                             asNoTracking: false
                );
            List<Class> subjectClass = subj.Classes.ToList();
            List<LearningOutcomes> learningOutComes = subj.LearingOutcomes.ToList();
            List<Question> questions = learningOutComes.SelectMany(lrn => lrn.Questions).ToList();
            List<Answer> answers = questions.SelectMany(q=> q.Answers).ToList();
            List<StudentAnswers> studentAnswers = questions.SelectMany(q=> q.StudentAnswers).ToList();

          
            
            _studentAnswers.RemoveRange(studentAnswers);
            foreach (Exam ex in subj.Exams)
            {
                await _deleteService.deleteExam(ex);
            }
            _answerRepo.RemoveRange(answers);
            _questionRepo.RemoveRange(questions);
            _learningOutComesRepo.RemoveRange(learningOutComes);

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

                if (roles.Contains(Roles.Admin.ToString()))
                {
                subjects = _subjectRepo.GetAllAsync().Result.Select(s => new SubjectViewDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                }).ToList();

                    return subjects;
                }
                return subjects;
            
        }
        public async Task<SubjectViewDetailsDto> GetSubjectByIdAsync(int id)
        {
            var subject = await _subjectRepo.GetIncludeById(id, "LearingOutcomes");
            if (subject == null)
            {
                throw new ArgumentException("A Subject not found.");
            }

                return new SubjectViewDetailsDto()
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    Description = subject.Description,
                    LearingOutComes = subject.LearingOutcomes.Select(l => $"ID: {l.Id}, Title: {l.Title}, Description: {l.Description}")
                    .ToList()

                };

        }
        public async Task<List<SubjectViewDetailsDto>> GetSubjectForTeacher(string teacherEmail,int departmentId)
        {
            var user = await _userManger.FindByEmailAsync(teacherEmail);
            var roles = await _userManger.GetRolesAsync(user);

            if (roles.Contains(Roles.Teacher.ToString()))
            {
                List<Class> classes = await _classRepo.GetAllIncludeAsync("Subject");
                List<Subject> subjects = classes.Where(c=> c.TeacherId == user.Id && c.DepartmentId == departmentId).Select(c => c.Subject)
            .Where(s => s != null)
            .DistinctBy(s => s.Id)
            .ToList();
                return subjects.Select(s=> new SubjectViewDetailsDto 
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description= s.Description
                }).ToList();
            }
            return null;
        }

    }
}
