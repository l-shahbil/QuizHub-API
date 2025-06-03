using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.LearingOutComes;
using QuizHub.Models.DTO.Question;
using QuizHub.Models.DTO.Subject;
using QuizHub.Services.Admin_Services.Interface;

namespace QuizHub.Services.Admin_Services
{
    public class LearningOutComesService : ILearningOutComesService
    {
        private readonly IRepository<LearningOutcomes> _learningOutComesRepo;
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<StudentAnswers> _studentAnswerRepo;
        private readonly IRepository<Answer> _answerRepo;

        public LearningOutComesService(IRepository<LearningOutcomes> learningOutComesRepo,IRepository<Subject>subjectRepo,IRepository<Question> questionRepo
            ,IRepository<StudentAnswers> studentAnswerRepo,IRepository<Answer> answerRepo)
        {
            _learningOutComesRepo = learningOutComesRepo;
            _subjectRepo = subjectRepo;
            _questionRepo = questionRepo;
            _studentAnswerRepo = studentAnswerRepo;
            _answerRepo = answerRepo;
        }
        public async Task<LearningOutComesViewDto> AddLearningOutComesAsync(int subjectId, LearningOutComesCreateDto model)
        {
            var existSubject = await _subjectRepo.SelecteOne(sub => sub.Id == subjectId);
            if (existSubject == null)
            {
                throw new ArgumentException($"A Subject with ID { subjectId } not found.");
            }

            var existLearningOutComes = await _learningOutComesRepo.SelecteOne(l => l.Title == model.Title);
            if (existLearningOutComes != null)
            {
                throw new ArgumentException($"A LearningOutComes with the same name already exists.");
            }
            var newLearingOutcomes = new LearningOutcomes()
            {
                Title = model.Title,
                Description = model.Description
                ,Subject = existSubject
            };
            await _learningOutComesRepo.AddAsyncEntity(newLearingOutcomes);
            return new LearningOutComesViewDto
            {
                Id = newLearingOutcomes.Id,
                Title = model.Title,
                Description = model.Description,

            };
        }

        public async Task<bool> DeleteLearningOutComesAsync(int id)
        {
            LearningOutcomes learingOutcomes = await _learningOutComesRepo.GetFirstOrDefaultAsync(
                    filter:l=> l.Id ==id,
                    include:query=> query.Include(l=> l.Questions).ThenInclude(q=> q.Answers)
                    .Include(l=> l.Questions).ThenInclude(q=> q.StudentAnswers)
                );
            if (learingOutcomes == null)
            {
                return false;
            }

            List<Question> questions = learingOutcomes.Questions.ToList();
            List<Answer> answers = questions.SelectMany(q => q.Answers).ToList();
            List<StudentAnswers> studentAnswers = questions.SelectMany(q => q.StudentAnswers).ToList();



            _studentAnswerRepo.RemoveRange(studentAnswers);
            //foreach (Exam ex in subj.Exams)
            //{
            //    await _deleteService.deleteExam(ex);
            //}
            _answerRepo.RemoveRange(answers);
            _questionRepo.RemoveRange(questions);
            _learningOutComesRepo.DeleteEntity(learingOutcomes);
            return true;
        }

        public async Task<LearningOutComesViewDto> EditLearningOutComesAsync(int id, LearningOutComesUpdateDto model)
        {

            var learingOutcomes = await _learningOutComesRepo.GetIncludeById(id, "Subject")
             ?? throw new KeyNotFoundException($"LearningOutComes with ID {id} not found.");
            var existLearningOutComes = await _learningOutComesRepo.SelecteOne(l => l.Title == model.Title);
            if (existLearningOutComes != null)
            {
                throw new ArgumentException("A LearningOutComes with the same name already exists.");
            }
            learingOutcomes.Title = string.IsNullOrWhiteSpace(model.Title) ? learingOutcomes.Title : model.Title;
            learingOutcomes.Description = string.IsNullOrWhiteSpace(model.Description) ? learingOutcomes.Description : model.Description;

            _learningOutComesRepo.UpdateEntity(learingOutcomes);
            return new LearningOutComesViewDto
            {
                Id = learingOutcomes.Id,
                Title = learingOutcomes.Title,
                Description = learingOutcomes.Description,

            };
        }

        public async Task<List<LearningOutComesViewDto>> GetAllLearningOutComesAsync(int subjectId)
        {
            var existSubject = await _subjectRepo.GetByIdAsync(subjectId);
            if (existSubject == null)
            {
                throw new KeyNotFoundException($"A Subject with ID {subjectId} not found.");
            }

            List<LearningOutcomes> allLearningOutComes = await _learningOutComesRepo.GetAllIncludeAsync("Questions");

            var learningOutComes = allLearningOutComes.Where(l=> l.subjectId == subjectId).Select(l => new LearningOutComesViewDto
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                QuestionCount = l.Questions.Count()

            }).ToList();
            return learningOutComes;
        }

        public async Task<LearningOutComesViewDto> GetLearningOutComesByIdAsync(int id)
        {
            var leaerningOutComes = await _learningOutComesRepo.GetIncludeById(id, "Questions", "Subject");
            if (leaerningOutComes == null)
            {

                throw new ArgumentException("A Subject not found.");
            }
            return new LearningOutComesViewDto
            {
                Id = leaerningOutComes.Id,
                Title = leaerningOutComes.Title,
                Description = leaerningOutComes.Description,
                questions = leaerningOutComes.Questions
            .Select(q => new QuestionViewDto{QuestionId = q.Id,QuestionText = q.QuestionText, Discrimination =q.Discrimination, Difficulty =q.Difficulty})
            .ToList()
            };
        }
    }
}
