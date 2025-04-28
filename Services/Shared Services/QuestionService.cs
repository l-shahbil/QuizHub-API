using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Notification;
using QuizHub.Models.DTO.Question;
using QuizHub.Services.Shared_Services.Interface;
using QuizHub.Services.SubAdmin_Services.Interface;
using QuizHub.Models.DTO.Answer;


namespace QuizHub.Services.Shared_Services
{
    public class QuestionService : IQuestionService
    {


        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Class> _calssRepo;
        private readonly IRepository<LearningOutcomes> _learningOutComesRepo;
        private readonly IAnswerService _answerService;

        public QuestionService(UserManager<AppUser> userManager, IRepository<Question> questionRepo,IRepository<Subject>subjectRepo, IRepository<Department> departmentRepo, IRepository<Class> calssRepo,IRepository<LearningOutcomes> learningOutComesRepo,IAnswerService answerService)
        {
            _userManager = userManager;
            _questionRepo = questionRepo;
            _subjectRepo = subjectRepo;
            _departmentRepo = departmentRepo;
            _calssRepo = calssRepo;
            _learningOutComesRepo = learningOutComesRepo;
            _answerService = answerService;
        }


        public async Task<QuestionViewDto> AddQuestionAsync(string userEmail, int departmentId, int learningOutComesId, QuestionCreateDto model,int? classId)
        {
           
            LearningOutcomes lrn = await _learningOutComesRepo.GetByIdAsync(learningOutComesId);
            if (lrn == null)
            {
                throw new KeyNotFoundException($"A LearnOutComes with ID {learningOutComesId} not found.");
            }

            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                var existDepartment = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin", "Subjects");
                if (existDepartment == null)
                {
                    throw new KeyNotFoundException($"A Department with ID {departmentId} not found.");
                }
                if (existDepartment.SubAdmin.Email != userEmail)
                {
                    throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
                }

                var isSubjectiAsseignedToDepartment = existDepartment.Subjects.Any(s => s.Id == lrn.subjectId);
                if (!isSubjectiAsseignedToDepartment)
                {
                    throw new Exception("The subject not assigned to department.");
                }

                if ( _answerService.ValidateAnswers(model.Answers))
                {
                    //because the validateAnswers have a try and catch So we don't need to check the condition. this is middleware
                }

                Question question = new Question()
                {
                    QuestionText = model.QuestionText,
                    Difficulty = model.Difficulty,
                    Discrimination = model.Discrimination,
                    leraningOutComes = lrn,
                    User = user,
                    
                    CreatedAt = DateTime.Now,

                };
                await _questionRepo.AddAsyncEntity(question);
                foreach(var n in model.Answers)
                {
                   await  _answerService.addAnswersAsync(question,n);
                }
                return new QuestionViewDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    Difficulty = question.Difficulty,
                    Discrimination = question.Discrimination,
                    Answers = question.Answers.Select(a => new AnswerViewDto {
                        Id = a.Id,
                        AnswerText =a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                };

            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                Class cls =await _calssRepo.GetByIdAsync(classId);
                if (cls == null)
                {
                    throw new KeyNotFoundException($"A Class with ID {classId} not found.");
                }
                if(cls.SubjectId != lrn.subjectId)
                {
                    throw new Exception("The subject associated with this class does not match the subject of the Learning Outcomes.");
                }
                if (cls.TeacherId != user.Id)
                {
                    throw new UnauthorizedAccessException("Access Denied: You are not the assigned Teacher for this Class.");

                }
                if (_answerService.ValidateAnswers(model.Answers))
                {
                    //because the validateAnswers have a try and catch So we don't need to check the condition. this is middleware
                }
                Question question = new Question()
                {
                    QuestionText = model.QuestionText,
                    Difficulty = model.Difficulty,
                    Discrimination = model.Discrimination,
                    User = user,
                    leraningOutComes = lrn,
                    CreatedAt = DateTime.Now,

                };
                await _questionRepo.AddAsyncEntity(question);
                foreach (var n in model.Answers)
                {
                    await _answerService.addAnswersAsync(question, n);
                }
                return new QuestionViewDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    Difficulty = question.Difficulty,
                    Discrimination = question.Discrimination,
                    Answers = question.Answers.Select(a => new AnswerViewDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                };
            }

            return null;
        }

        public async Task<bool> DeleteQuestionAsync(string userEmail, int questionId)
        {
            Question question = await _questionRepo.GetIncludeById(questionId);
            if (question == null)
            {
                throw new KeyNotFoundException($"A Question with ID {questionId} not found.");
            }

            AppUser user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                _questionRepo.DeleteEntity(question);
                return true;
            }
            else if (roles.Contains(Roles.Teacher.ToString())) {
                if(question.userId != user.Id)
                {
                    throw new Exception("You are not authorized to perform this action on this question.");
                }
                _questionRepo.DeleteEntity(question);
                return true;
            }
            return false;
        }

        public async Task<QuestionViewDto> EditQuestionAsync(string userEmail, int questionId, QuestionUpdateDto model)
        {
            Question question = await _questionRepo.GetIncludeById(questionId, "Answers");
            if(question == null)
            {
                throw new KeyNotFoundException($"A Question with ID {questionId} not found.");
            }

            AppUser user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);

            //update the data but that isn't in data base
            question.QuestionText = string.IsNullOrWhiteSpace(model.QuestionText) ? question.QuestionText : model.QuestionText;
            if (model.Difficulty != 0)
            {
                question.Difficulty = model.Difficulty;
            }
            if (model.Discrimination != 0)
            {
                question.Discrimination = model.Discrimination;
            }

            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                _questionRepo.UpdateEntity(question);

                return new QuestionViewDto{ 
                    QuestionId = questionId,
                    QuestionText = question.QuestionText,
                    Difficulty = question.Difficulty,
                    Discrimination = question.Discrimination,
                    Answers = question.Answers.Select(a => new AnswerViewDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                };
            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                if (question.userId != user.Id)
                {
                    throw new Exception("You are not authorized to perform this action on this question.");
                }


                _questionRepo.UpdateEntity(question);

                return new QuestionViewDto
                {
                    QuestionId = questionId,
                    QuestionText = question.QuestionText,
                    Difficulty = question.Difficulty,
                    Answers = question.Answers.Select(a => new AnswerViewDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                };
               
            }
            return null;
        }

        public async Task<List<QuestionViewDto>> GetAllQuestion(string userEmail,int subjectId)
        {
            Subject subject = await _subjectRepo.GetIncludeById(subjectId, "LearingOutcomes");
            if(subject == null)
            {
                throw new KeyNotFoundException($"A Subject with ID {subject} not found.");

            }


            List<QuestionViewDto> questions = _questionRepo.GetAllIncludeAsync("leraningOutComes", "Answers").Result
                .Where(q => subject.LearingOutcomes.Contains(q.leraningOutComes))
                .Select(q => new QuestionViewDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    Difficulty = q.Difficulty,
                    Discrimination = q.Discrimination,
                    Answers = q.Answers.Select(a => new AnswerViewDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList();
               return questions;
        }

        public async Task<QuestionViewDto> GetQuestionById( int questionId)
        {
            Question question = await _questionRepo.GetIncludeById(questionId, "Answers");
            if (question == null)
            {
                throw new KeyNotFoundException($"A Question with ID {questionId} not found.");
            }
            return new QuestionViewDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                Difficulty = question.Difficulty,
                Discrimination = question.Discrimination,
                Answers = question.Answers.Select(ans => new AnswerViewDto
                {
                    Id = ans.Id,
                    AnswerText = ans.AnswerText,
                    IsCorrect = ans.IsCorrect
                }).ToList()
            };
        }
    }
}
