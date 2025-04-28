using Azure.Core;
using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Answer;
using QuizHub.Models.DTO.Exam;
using QuizHub.Models.DTO.Question;
using QuizHub.Services.Shared_Services.Interface;
using System;

namespace QuizHub.Services.Shared_Services
{
    public class ExamService : IExamService
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Exam> _examRepo;
        private readonly IRepository<Question> _questionRepo;
        private readonly IExamValidator _examValidation;
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Class> _classRepo;
        private readonly IRepository<LearningOutcomes> _learningOutComesRepo;
        private readonly IRepository<ExamQuestion> _examQuestionRepo;
        private readonly IRepository<ClassExam> _classExamRepo;

        public ExamService(UserManager<AppUser> userManager,IRepository<Exam> examRepo, IRepository<Question> questionRepo
            ,IExamValidator examValidation,IRepository<Subject> subjectRepo, IRepository<Department> departmentRepo,
            IRepository<Class> classRepo, IRepository<LearningOutcomes> learningOutComesRepo,
            IRepository<ExamQuestion> examQuestionRepo,IRepository<ClassExam> classExamRepo)
        {
            _userManager = userManager;
            _examRepo = examRepo;
            _questionRepo = questionRepo;
            _examValidation = examValidation;
            _subjectRepo = subjectRepo;
            _departmentRepo = departmentRepo;
            _classRepo = classRepo;
            _learningOutComesRepo = learningOutComesRepo;
            _examQuestionRepo = examQuestionRepo;
            _classExamRepo = classExamRepo;
        }
        public async Task<ExamViewDto> CreateExamAsync(string userEmail, int departmentId, ExamCreateDto model)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            var subject = await _subjectRepo.GetIncludeById(model.SubjectId, "LearingOutcomes");

            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                //this constrant for create exam ;this all return true or false
                await _examValidation.CheckSubAdminOwnershipDepartment(userEmail, departmentId);
                await _examValidation.IsSubjectAssignedToDepartment(model.SubjectId, departmentId);
                await _examValidation.IsLearningOutcomesFounded(model.SubjectId, model.learningOutcomeIds);
                ExamViewDto examViewDto =await generateExam(model, subject, user);

                return examViewDto;

            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                await _examValidation.IsTheClassInThisDepartment(departmentId, model.classId);
                await _examValidation.IsSubjectAssignedToDepartment(model.SubjectId, departmentId);
                await _examValidation.IsTheTeacherLinkedToTheSubject(user.Id, model.classId, model.SubjectId);

                ExamViewDto examViewDto = await generateExam(model, subject, user);

                return examViewDto;
            }
            return null;

        }

        public Task<bool> DeleteExamAsync(string userEmail, int questionId)
        {
            throw new NotImplementedException();
        }

        public Task DisplayExamAttendenceAndResults()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExamPuplish(string userEmail,string examId,int classId,ExamPublishDto model)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            Exam exam = await _examRepo.GetByIdAsync(examId);
            Class cls = await _classRepo.GetByIdAsync(classId);

            if (exam == null) {
                throw new InvalidOperationException($"Exam with ID {examId} was not found.");
            }

            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                //this constrant for create exam ;this all return true or false
                await _examValidation.CheckSubAdminOwnershipClass(user.Id,classId);
                await _examValidation.IsSubjectAssignedToTheClass(classId,exam.SubjectId);

                ClassExam clsExam = new ClassExam
                {
                    Exam = exam,
                    Class =cls,
                    Score = model.Score,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Duration = model.Duration
                };

                await _classExamRepo.AddAsyncEntity(clsExam);
                return true;
            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                await _examValidation.IsTheTeacherLinkedToTheSubject(user.Id,classId,exam.SubjectId);
                ClassExam clsExam = new ClassExam
                {
                    Exam = exam,
                    Class = cls,
                    Score = model.Score,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Duration = model.Duration
                };

                await _classExamRepo.AddAsyncEntity(clsExam);
                return true;

            }
            return false;
        }
        public async Task<bool> CancelExamPublication(string userEmail, int classExamId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            ClassExam clsExam = await _classExamRepo.GetByIdAsync(classExamId);
            if (clsExam == null) 
            {
                throw new InvalidOperationException($"Exam with ID {classExamId} was not found.");
            }

            Class cls = await _classRepo.GetIncludeById(clsExam.ClassId, "Department");

            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                await _examValidation.CheckSubAdminOwnershipClass(user.Id, cls.Id);

                _classExamRepo.DeleteEntity(clsExam);
                return true;
            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                await _examValidation.IsTheTeacherLinkedToTheSubject(user.Id, cls.Id, cls.SubjectId);
                _classExamRepo.DeleteEntity(clsExam);
                return true;
            }
            return false;
        }

        public Task ExamSubmission()
        {
            throw new NotImplementedException();
        }

        public Task<ExamViewDto> ExamTake()
        {
            throw new NotImplementedException();
        }

        public Task<List<ExamViewDto>> GetAllExams(string userEmail, int subjectId)
        {
            throw new NotImplementedException();
        }

        public Task<ExamViewDto> GetExamById(int questionId)
        {
            throw new NotImplementedException();
        }

        public Task GetExamPrevious()
        {
            throw new NotImplementedException();
        }

        public Task GetExamsAvalibie()
        {
            throw new NotImplementedException();
        }

        public Task ViewExamResult()
        {
            throw new NotImplementedException();
        }
        private async Task<ExamViewDto> generateExam(ExamCreateDto model,Subject subject ,AppUser user)
        {
            var initQS = initQuestion(model.learningOutcomeIds, model.NumberOfEasyQuestions, model.NumberOfMediumLevelQuestions, model.NumberOfDifficultQuestions, model.ClarityRangeFrom, model.ClarityRangeTo);

            Exam exam = new Exam
            {
                Id = Guid.NewGuid().ToString(),
                Title = model.Title,
                Description = model.Description,
                Subject = subject,
                CreatedDate = DateTime.Now,
                AppUser = user
            };
            await _examRepo.AddAsyncEntity(exam);
            foreach (var q in initQS)
            {
                ExamQuestion examQuestion = new ExamQuestion
                {
                    Exam = exam,
                    Question = q
                };

                await _examQuestionRepo.AddAsyncEntity(examQuestion);
            }
            return new ExamViewDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                CreatedDate = exam.CreatedDate,
                userName = user.Email,
                Subject = subject.Name,
                learningOutComes = subject.LearingOutcomes.Select(l=> l.Title).ToList(),
                questions = initQS.Select(q => new QuestionViewDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    Difficulty = q.Difficulty,
                    Discrimination = q.Discrimination,
                    Answers = q.Answers.Select(ans => new AnswerViewDto
                    {
                        Id = ans.Id,
                        AnswerText = ans.AnswerText,
                        IsCorrect = ans.IsCorrect
                    }).ToList()
                }).ToList()
            };
        }
        private List<Question> initQuestion(List<int> learningOutComesIDs, int easyCount, int mediumCount, int hardCount, decimal clarityRangeFrom, decimal clarityRangeTo)
        {
            // # step-1
                //1. اختيار الاسئلة بناءا على المادة ومخرجات التعلم
            var questionByLearningOutComes = FilterQuestionsByLearningOutComes(learningOutComesIDs);
                //2.اختيار الاسئلة بناءا على نطاق التميز المحدد

            var questionsInRange = FilterQuestionsByClarityRange(questionByLearningOutComes,clarityRangeFrom,clarityRangeTo);

            if (!questionsInRange.Any())
            {
                throw new InvalidOperationException("No questions found in the specified clarity range.");
            }

                // 3. تقسيم حسب الصعوبة
            var easyQuestions = FilterByDifficulty(questionsInRange, 0, 0.4m);
            var mediumQuestions = FilterByDifficulty(questionsInRange, 0.4m, 0.7m);
            var hardQuestions = FilterByDifficulty(questionsInRange, 0.7m, 0.1m);

            ValidateAvailability(easyQuestions, easyCount, "easy");
            ValidateAvailability(mediumQuestions,mediumCount, "medium");
            ValidateAvailability(hardQuestions, hardCount, "hard");



            // # setp-2
            var random = new Random();
            var selectedEasy = easyQuestions.OrderBy(q => random.Next()).Take(easyCount).ToList();
            var selectedMedium = mediumQuestions.OrderBy(q => random.Next()).Take(mediumCount).ToList();
            var selectedHard = hardQuestions.OrderBy(q => random.Next()).Take(hardCount).ToList();

            // # step-3
            var finalExamQuestions = new List<Question>();
            finalExamQuestions.AddRange(selectedEasy);
            finalExamQuestions.AddRange(selectedMedium);
            finalExamQuestions.AddRange(selectedHard);

            // 6. خلط نهائي للامتحان
            return ShuffleQuestions(finalExamQuestions);
        }




        private List<Question> FilterQuestionsByLearningOutComes(List<int> learningOutComesIDs)
        {
            var questions = _learningOutComesRepo.GetAllIncludeAsync("Questions").Result.Where(l => learningOutComesIDs.Contains(l.Id)).SelectMany(l => l.Questions);
            return questions.ToList();
        }
        private List<Question> FilterQuestionsByClarityRange(List<Question> questions, decimal clarityRangeFrom, decimal clarityRangeTo)
        {
            if (clarityRangeFrom > clarityRangeTo)
            {
                throw new ArgumentException("ClarityRangeFrom must be less than ClarityRangeTo.");
            }
            return questions.Where(q => q.Discrimination >= clarityRangeFrom && q.Discrimination <= clarityRangeTo).ToList();
        }
        private List<Question> FilterByDifficulty(List<Question> questions, decimal difficultyFrom, decimal difficultyTo)
        {
            return questions
                .Where(q => q.Difficulty > difficultyFrom && q.Difficulty <= difficultyTo)
                .ToList();
        }
        private List<Question> ShuffleQuestions(List<Question> questions)
        {

            var random = new Random();
            var questionWithOrder=  questions.OrderBy(q => random.Next()).ToList();

            var questionsWithAnswer = _questionRepo.GetAllIncludeAsync("Answers").Result.Where(q=> questionWithOrder.Contains(q));
            return questionsWithAnswer.ToList();
        }

        private void ValidateAvailability(List<Question> questions, int requiredCount, string difficulty)
        {
            if (questions.Count < requiredCount)
                throw new InvalidOperationException($"Not enough {difficulty} questions available.");
        }
    }
}
