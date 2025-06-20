﻿using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Notification;
using QuizHub.Models.DTO.Question;
using QuizHub.Services.Shared_Services.Interface;
using QuizHub.Services.SubAdmin_Services.Interface;
using QuizHub.Models.DTO.Answer;
using static QuizHub.Services.Shared_Services.QuestionService;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;


namespace QuizHub.Services.Shared_Services
{
    public class QuestionService : IQuestionService
    {


        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<Exam> _examRepo;
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Class> _calssRepo;
        private readonly IRepository<LearningOutcomes> _learningOutComesRepo;
        private readonly IAnswerService _answerService;
        private readonly HttpClient _httpClient;
        private readonly string _modeApilUrl;

        public QuestionService(UserManager<AppUser> userManager, IRepository<Question> questionRepo,IRepository<Exam> examRepo,
            IRepository<Subject>subjectRepo, IRepository<Department> departmentRepo, IRepository<Class> calssRepo
            ,IRepository<LearningOutcomes> learningOutComesRepo,IAnswerService answerService, HttpClient httpClient, IConfiguration configuration)
        {
            _userManager = userManager;
            _questionRepo = questionRepo;
            _examRepo = examRepo;
            _subjectRepo = subjectRepo;
            _departmentRepo = departmentRepo;
            _calssRepo = calssRepo;
            _learningOutComesRepo = learningOutComesRepo;
            _answerService = answerService;
            _httpClient = httpClient;
            _modeApilUrl = configuration["ExternalServices:FlaskModelApiBaseUrl"];
        }


        public async Task<QuestionViewDto> AddQuestionAsync(string userEmail, int departmentId, int learningOutComesId, QuestionCreateDto model)
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

                if (!(model.Discrimination > 0 && model.Discrimination <= 1))
                {
                    throw new ArgumentOutOfRangeException(nameof(model.Discrimination), "Discrimination must be greater than zero and smaller than 100%.");

                }


                float difficulty = await PredictDifficultyModelAsync(model.QuestionText);


                Question question = new Question()
                {
                    QuestionText = model.QuestionText,
                    Difficulty =Convert.ToDecimal(difficulty),
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
                List<Class> clases =await _calssRepo.GetAllAsync();
                Class cls = clases.FirstOrDefault(c=> c.SubjectId == lrn.subjectId && c.TeacherId == user.Id);
                if (cls == null)
                {
                    throw new UnauthorizedAccessException($"The Teacher Not Linked to subject.");
                }
                if (_answerService.ValidateAnswers(model.Answers))
                {
                    //because the validateAnswers have a try and catch So we don't need to check the condition. this is middleware
                }
                float difficulty = await PredictDifficultyModelAsync(model.QuestionText);

                Question question = new Question()
                {
                    QuestionText = model.QuestionText,
                    Difficulty = Convert.ToDecimal(difficulty),
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
            Question qest = await _questionRepo.GetFirstOrDefaultAsync(
                filter: q => q.Id == questionId,
                include: query => query.Include(q=> q.ExamQuestions).ThenInclude(Ex=> Ex.Exam)
                );

            List<Exam> exams = question.ExamQuestions
                .Select(exq => exq.Exam)
                .Where(ex => ex != null)
                .ToList();

            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                foreach(Exam ex in exams)
                {
                    ex.questionCount -= 1;

                    if (question.Difficulty < Convert.ToDecimal(0.40))
                    {
                        ex.NumberOfEasyQuestions -= 1;
                    }
                    else if (question.Difficulty < Convert.ToDecimal(0.69))
                    {
                        ex.NumberOfMediumLevelQuestions -= 1;
                    }
                    else if (question.Difficulty > Convert.ToDecimal(0.69))
                    {
                        ex.NumberOfDifficultQuestions -= 1;
                    }

                     _examRepo.UpdateEntity(ex);
                }
                


                    _questionRepo.DeleteEntity(question);
                return true;
            }
            else if (roles.Contains(Roles.Teacher.ToString())) {
                if(question.userId != user.Id)
                {
                    throw new UnauthorizedAccessException("You are not authorized to perform this action on this question.");
                }

                foreach (Exam ex in exams)
                {
                    ex.questionCount -= 1;

                    if (question.Difficulty < Convert.ToDecimal(0.40))
                    {
                        ex.NumberOfEasyQuestions -= 1;
                    }
                    else if (question.Difficulty < Convert.ToDecimal(0.69))
                    {
                        ex.NumberOfMediumLevelQuestions -= 1;
                    }
                    else if (question.Difficulty > Convert.ToDecimal(0.69))
                    {
                        ex.NumberOfDifficultQuestions -= 1;
                    }

                    _examRepo.UpdateEntity(ex);
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


            if (!(model.Discrimination > 0 && model.Discrimination <=1))
            {
                throw new ArgumentOutOfRangeException(nameof(model.Discrimination), "Discrimination must be greater than zero and smaller than 100%.");

            }
          
                question.Discrimination = model.Discrimination;

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
        private async Task<float> PredictDifficultyModelAsync(string questionText)
        {
            var requestData = new { text = questionText };
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            try
            {
                var response = await _httpClient.PostAsync(
                    _modeApilUrl,
                    content,cts.Token);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception(result);

                var jsonDocument = JsonDocument.Parse(result);

                if (jsonDocument.RootElement.TryGetProperty("difficulty_score", out var difficultyScoreElement))
                {
                    var difficultyScore = difficultyScoreElement.GetSingle();

                    if (difficultyScore > 0.44f)
                    {
                        difficultyScore *= 1.3f;
                        if (difficultyScore > 1f)
                            difficultyScore = 1f;
                    }

                    return difficultyScore;
                }

                if (jsonDocument.RootElement.TryGetProperty("error", out var errorElement))
                    throw new Exception(errorElement.GetString());

                throw new Exception("Unexpected response from Flask API.");
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("Request to Flask API timed out.", ex);
            }
        }


    }
}
