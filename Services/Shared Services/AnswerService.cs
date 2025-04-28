using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Answer;
using QuizHub.Services.Shared_Services.Interface;

namespace QuizHub.Services.Shared_Services
{
    public class AnswerService : IAnswerService
    {
        private readonly IRepository<Answer> _answerRepo;
        private readonly IRepository<Question> _questionRepo;
        private readonly UserManager<AppUser> _userManager;

        public AnswerService(IRepository<Answer> answerRepo,IRepository<Question> questionRepo,UserManager<AppUser> userManger)
        {
            _answerRepo = answerRepo;
            _questionRepo = questionRepo;
            _userManager = userManger;
        }
        public async Task addAnswersAsync( Question question,AnswerCreateDto model)
        {

            try
            {
               
                Answer answer = new Answer()
                {
                    AnswerText = model.AnswerText,
                    IsCorrect = model.IsCorrect,
                    Question = question
                };
                await _answerRepo.AddAsyncEntity(answer);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public async Task<AnswerViewDto> addAnswerAsync(string userEmail, int questionId, AnswerCreateDto model)
        {
            Question question = await _questionRepo.GetIncludeById(questionId, "Answers");
            if (question == null)
            {
                throw new KeyNotFoundException($"A Question with ID {questionId} not found.");
            }

            if (question.Answers.Count == 4)
            {
                throw new ArgumentException("The number of answers has exceeded four. If you want to add a new answer, you must delete an existing one first.");
            }

            if(model.IsCorrect == true)
            {
            var countCheck = 0;
            foreach (var n in question.Answers)
            {
                if (n.IsCorrect == true)
                {
                    countCheck += 1;
                }
            }
            if (countCheck == 1)
            {
                    throw new Exception("A question must have only one correct answer. Please delete the existing correct answer and add the new one.");

                }
            }

            Answer answer = new Answer
            {
                AnswerText = model.AnswerText,
                IsCorrect = model.IsCorrect,
            };


            AppUser user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
               await  _answerRepo.AddAsyncEntity(answer);
                return new AnswerViewDto{
                    Id = answer.Id,
                    AnswerText = answer.AnswerText,
                    IsCorrect = answer.IsCorrect
                };
            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                if (question.userId != user.Id)
                {
                    throw new Exception("You are not authorized to perform this action on this question.");
                }
                await _answerRepo.AddAsyncEntity(answer);
                return new AnswerViewDto
                {
                    Id = answer.Id,
                    AnswerText = answer.AnswerText,
                    IsCorrect = answer.IsCorrect
                };
            }
            return null;
        }

        public async Task<bool> DeleteAnswerAsync(string userEmail,int questionId, int answerId)
        {
            Question question = await _questionRepo.GetIncludeById(questionId);
            if (question == null)
            {
                throw new KeyNotFoundException($"A Question with ID {questionId} not found.");
            }


            Answer answer = await _answerRepo.GetByIdAsync(answerId);
            if (answer == null) {
               
                    throw new KeyNotFoundException($"A Answer with ID {answerId} not found.");
                
            }
            if (answer.QuestionId != questionId) {
                throw new ArgumentNullException("Please make sure to verify the question ID. The provided answer does not belong to the specified question.");
            }

            AppUser user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                _answerRepo.DeleteEntity(answer);
                return true;
            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                if (question.userId != user.Id)
                {
                    throw new Exception("You are not authorized to perform this action on this question.");
                }
                _answerRepo.DeleteEntity(answer);
                return true;
            }
            return false;

        }

        public async Task<AnswerViewDto> EditAnswerAsync(string userEmail, int questionId,int answerId, AnswerEditDto model)
        {
            Question question = await _questionRepo.GetIncludeById(questionId);
            if (question == null)
            {
                throw new KeyNotFoundException($"A Question with ID {questionId} not found.");
            }


            Answer answer = await _answerRepo.GetByIdAsync(answerId);
            if (answer == null)
            {

                throw new KeyNotFoundException($"A Answer with ID {answerId} not found.");

            }
            if (answer.QuestionId != questionId)
            {
                throw new ArgumentNullException("Please make sure to verify the question ID. The provided answer does not belong to the specified question.");
            }

            AppUser user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            
            answer.AnswerText =string.IsNullOrWhiteSpace(model.AnswerText)? answer.AnswerText:model.AnswerText;
            if(answer.IsCorrect == model.IsCorrect)
            {
                answer.IsCorrect = model.IsCorrect;
            }


            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                _answerRepo.UpdateEntity(answer);
                return new AnswerViewDto{
                    Id = answer.Id,
                    AnswerText = answer.AnswerText,
                    IsCorrect = answer.IsCorrect,
                };
            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                if (question.userId != user.Id)
                {
                    throw new Exception("You are not authorized to perform this action on this question.");
                }
                _answerRepo.UpdateEntity(answer);
                return new AnswerViewDto
                {
                    Id = answer.Id,
                    AnswerText = answer.AnswerText,
                    IsCorrect = answer.IsCorrect,
                };
            }
            return null;
        }



       public bool ValidateAnswers(List<AnswerCreateDto> Answers)
        {
            //Check Of Answer
            if (Answers.Count != 4)
            {
                throw new Exception("A question must have exactly four answers.");
            }
            var countCheck = 0;
            foreach (var n in Answers)
            {
                if (n.IsCorrect == true)
                {
                    countCheck += 1;
                }
            }
            if (countCheck != 1)
            {
                throw new Exception("A question must have only one correct answer.");

            }

            return true;
           
        }
    }
}
