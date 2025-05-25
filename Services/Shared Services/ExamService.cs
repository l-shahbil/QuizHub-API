using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Migrations;
using QuizHub.Models;
using QuizHub.Models.DTO.Answer;
using QuizHub.Models.DTO.Exam;
using QuizHub.Models.DTO.Question;
using QuizHub.Services.Shared_Services.Interface;
using QuizHub.Utils.Interface;
using System;
using System.IO;

namespace QuizHub.Utils
{
    public class ExamService : IExamService
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Exam> _examRepo;
        private readonly IRepository<Question> _questionRepo;
        private readonly IExamValidator _examValidation;
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<StudentAnswers> _studentAnswerRepo;
        private readonly IRepository<StudentClass> _studentClassRepo;
        private readonly IRepository<Class> _classRepo;
        private readonly IRepository<LearningOutcomes> _learningOutComesRepo;
        private readonly IRepository<Answer> _answerRepo;
        private readonly IRepository<ExamQuestion> _examQuestionRepo;
        private readonly IRepository<ClassExam> _classExamRepo;
        private readonly IRepository<StudentExam> _studentExamRepo;
        private readonly IDeleteService _deleteService;

        public ExamService(UserManager<AppUser> userManager, IRepository<Exam> examRepo, IRepository<Question> questionRepo
            , IExamValidator examValidation, IRepository<Subject> subjectRepo, IRepository<Department> departmentRepo,
            IRepository<StudentAnswers> studentAnswerRepo,IRepository<StudentClass> studentClassRepo,
            IRepository<Class> classRepo, IRepository<LearningOutcomes> learningOutComesRepo, IRepository<Answer> answerRepo,
            IRepository<ExamQuestion> examQuestionRepo, IRepository<ClassExam> classExamRepo, 
            IRepository<StudentExam> studentExamRepo,IDeleteService deleteService)
        {
            _userManager = userManager;
            _examRepo = examRepo;
            _questionRepo = questionRepo;
            _examValidation = examValidation;
            _subjectRepo = subjectRepo;
            _departmentRepo = departmentRepo;
            _studentAnswerRepo = studentAnswerRepo;
            _studentClassRepo = studentClassRepo;
            _classRepo = classRepo;
            _learningOutComesRepo = learningOutComesRepo;
            _answerRepo = answerRepo;
            _examQuestionRepo = examQuestionRepo;
            _classExamRepo = classExamRepo;
            _studentExamRepo = studentExamRepo;
            _deleteService = deleteService;
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
                ExamViewDto examViewDto = await generateExam(model, subject, user);

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
        public async Task<ExamViewDto> UpdateExamAsynct(string userEmail, int departmentId, string examId, ExamUpdateDto model)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            Exam exam = await _examRepo.GetIncludeById(examId);
            if (exam == null)
            {
                throw new InvalidOperationException($"exam with ID {examId} was not found.");
            }



            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                await _examValidation.CheckSubAdminOwnershipDepartment(userEmail, departmentId);
                await _examValidation.IsSubjectAssignedToDepartment(exam.SubjectId, departmentId);


                exam.Title = string.IsNullOrWhiteSpace(model.Title) ? exam.Title : model.Title;
                exam.Description = string.IsNullOrWhiteSpace(model.Description) ? exam.Description : model.Description;

                _examRepo.UpdateEntity(exam);

                return new ExamViewDto
                {
                    Id = exam.Id,
                    Title = exam.Title,
                    Description = exam.Description,
                    CreatedDate = exam.CreatedDate
                };

            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                await _examValidation.IsSubjectAssignedToDepartment(exam.SubjectId, departmentId);
                await _examValidation.IsTheTeacherLinkedToTheSubject(user.Id, exam.SubjectId);



                exam.Title = string.IsNullOrWhiteSpace(model.Title) ? exam.Title : model.Title;
                exam.Description = string.IsNullOrWhiteSpace(model.Description) ? exam.Description : model.Description;

                _examRepo.UpdateEntity(exam);

                return new ExamViewDto
                {
                    Id = exam.Id,
                    Title = exam.Title,
                    Description = exam.Description,
                    CreatedDate = exam.CreatedDate
                };
            }
            return null;
        }
        public async Task<bool> DeleteExamAsync(string userEmail, string examId, int departmentId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            Exam exam = await _examRepo.GetByIdAsync(examId);
            if (exam == null)
            {
                throw new InvalidOperationException($"exam with ID {examId} was not found.");
            }



            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                await _examValidation.CheckSubAdminOwnershipDepartment(userEmail, departmentId);
                await _examValidation.IsSubjectAssignedToDepartment(exam.SubjectId, departmentId);


                await _deleteService.deleteExam(exam);
                return true;

            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                await _examValidation.IsSubjectAssignedToDepartment(exam.SubjectId, departmentId);
                await _examValidation.IsTheTeacherLinkedToTheSubject(user.Id, exam.SubjectId);
                if(exam.UserId != user.Id)
                {
                    throw new UnauthorizedAccessException("You are not authorized to delete this exam because you are not the creator.");

                }

                await _deleteService.deleteExam(exam);
                return true;
            }
            return false;
        }
        public async Task<bool> ExamPuplish(string userEmail, string examId, int classId, ExamPublishDto model)
        {
            _examValidation.validateDateRange(model.StartTime, model.EndTime);
            _examValidation.validateNonNegativeForScore(model.Score);

            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            Exam exam = await _examRepo.GetByIdAsync(examId);
            Class cls = await _classRepo.GetByIdAsync(classId);



            if (exam == null)
            {
                throw new InvalidOperationException($"Exam with ID {examId} was not found.");
            }

            if (cls == null)
            {
                throw new InvalidOperationException($"Class with ID {classId} was not found.");
            }

            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                //this constrant for create exam ;this all return true or false
                await _examValidation.CheckSubAdminOwnershipClass(user.Id, classId);
                await _examValidation.IsSubjectAssignedToTheClass(exam.SubjectId, classId);
                await _examValidation.isTheExamPublishedInTheClass(examId, classId, ExamFunctionType.publishExam);

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
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                await _examValidation.isTheTeacherLinkedToTheClass(user.Email, classId);
                await _examValidation.IsTheTeacherLinkedToTheSubject(user.Id, exam.SubjectId);
                await _examValidation.IsSubjectAssignedToTheClass(exam.SubjectId, classId);
                await _examValidation.isTheExamPublishedInTheClass(examId, classId, ExamFunctionType.publishExam);

                ClassExam clsExam = new ClassExam
                {
                    Exam = exam,
                    Class = cls,
                    Score = model.Score,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Duration = model.Duration,
                    showResult = model.showResult
                };


                await _classExamRepo.AddAsyncEntity(clsExam);

                return true;

            }
            return false;
        }
        public async Task<bool> CancelExamPublication(string userEmail, int classId, string examId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);

            try
            {

                if (roles.Contains(Roles.SubAdmin.ToString()))
                {
                    await _examValidation.CheckSubAdminOwnershipClass(user.Id, classId);
                    var clsExam = await _examValidation.isTheExamPublishedInTheClass(examId, classId, ExamFunctionType.cancelExam);


                    var classExam = await _classExamRepo.GetFirstOrDefaultAsync(
      ce => ce.ClassId == classId && ce.ExamId == examId,
      include: query => query.Include(ce => ce.StudentExam)
                            .ThenInclude(se => se.studentAnswers),
      asNoTracking: false
  );
                    var allAnswers = classExam.StudentExam.SelectMany(se => se.studentAnswers);
                    _studentAnswerRepo.RemoveRange(allAnswers);
                    _studentExamRepo.RemoveRange(classExam.StudentExam);
                    _classExamRepo.DeleteEntity(clsExam);
                    return true;
                }
                else if (roles.Contains(Roles.Teacher.ToString()))
                {

                    await _examValidation.isTheTeacherLinkedToTheClass(user.Email, classId);
                    var clsExam = await _examValidation.isTheExamPublishedInTheClass(examId, classId, ExamFunctionType.cancelExam);

                    var classExam = await _classExamRepo.GetFirstOrDefaultAsync(
      ce => ce.ClassId == classId && ce.ExamId == examId,
      include: query => query.Include(ce => ce.StudentExam)
                            .ThenInclude(se => se.studentAnswers),
      asNoTracking: false
  );
                    var allAnswers = classExam.StudentExam.SelectMany(se => se.studentAnswers);
                    _studentAnswerRepo.RemoveRange(allAnswers);
                    _studentExamRepo.RemoveRange(classExam.StudentExam);
                    _classExamRepo.DeleteEntity(clsExam);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        public async Task<ExamStudentViewDto> ExamTake(string userEmail, int classId, string examId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            Exam exam = await _examRepo.GetByIdAsync(examId);
            Class cls = await _classRepo.GetByIdAsync(classId);

            if (exam == null)
            {
                throw new ArgumentException($"Exam with ID {examId} was not found.");
            }

            if (cls == null)
            {
                throw new ArgumentException($"Class with ID {classId} was not found.");
            }

            //###### validation
            await _examValidation.isTheStudentLinkedToTheClass(user.Email, classId);
            ClassExam clsExam = await _examValidation.isTheExamPublishedInTheClass(exam.Id, cls.Id, ExamFunctionType.takeExam);
            _examValidation.isTheExamAvalible(clsExam);


            await _examValidation.HasStudentTakenExamAsync(user.Id, classId, examId);


            StudentExam stdExam = new StudentExam
            {
                id = Guid.NewGuid().ToString(),
                AttemptStartTime = DateTime.Now,
                Score = 0,
                User = user,
                clsExam = clsExam,
                ExamType = ExamType.Test.ToString()
            };
            await _studentExamRepo.AddAsyncEntity(stdExam);



            var examQuestions = await _examQuestionRepo.GetAllIncludeAsync("Question");
            var questions = examQuestions.Where(ex => ex.ExamId == exam.Id).Select(exQu => exQu.Question).ToList();

            return await getExamForStudent(stdExam.id, exam, questions, clsExam);

        }
        public async Task<ExamResultViewDto> ExamSubmission(string userEmail, ExamStudentSubmitDto model)
        {

            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);

            StudentExam stdExam = await _studentExamRepo.GetByIdAsync(model.stdExamId);


            if (stdExam == null)
            {
                throw new InvalidOperationException($"exam with ID {model.stdExamId} was not found.");
            }
            if (stdExam.isSubmit)
            {
                throw new InvalidOperationException($"you have already submitted the exam.");
            }

            var deliveryTime = DateTime.Now;
            var durationStudent = deliveryTime - stdExam.AttemptStartTime;

            var clsExams = await _classExamRepo.GetAllAsync();
            ClassExam clsExam = clsExams.FirstOrDefault(ce => ce.ExamId == stdExam.clsExamExamId && ce.ClassId == stdExam.clsExamClassId)!;


            try
            {
                await _examValidation.checkDateAndDuration(stdExam, durationStudent);
                stdExam.TimeComplation = durationStudent;
                stdExam.AttemptEndTime = deliveryTime;
                stdExam.isSubmit = true;

                var exam = await _examRepo.GetIncludeById(clsExam.ExamId, "Subject", "ExamQuestions");
                var answers = await _answerRepo.GetAllAsync();
                List<int> answersIDsForStudent = model.questions.Select(q => q.AnswerId).ToList();

                var answersForStudnet = answers.Where(a => answersIDsForStudent.Contains(a.Id));

                decimal markForEachQuestion = clsExam.Score / exam.questionCount;
                decimal totalScoreStudent = 0;

                foreach (var ans in answersForStudnet)
                {
                    StudentAnswers stdAnswer = new StudentAnswers
                    {
                        StudentExam = stdExam,
                        SelectAnswer = ans,
                        User = user,
                    };
                    if (ans.IsCorrect == true)
                    {

                        stdAnswer.IsCorrect = true;
                        stdAnswer.Score = markForEachQuestion;
                        totalScoreStudent += markForEachQuestion;
                    }
                    else if (ans.IsCorrect == false)
                    {
                        stdAnswer.IsCorrect = false;
                        stdAnswer.Score = 0;
                    }
                    await _studentAnswerRepo.AddAsyncEntity(stdAnswer);
                }

                stdExam.Score = totalScoreStudent;
                _studentExamRepo.UpdateEntity(stdExam);
                ExamResultViewDto examResult = await getExamResult(false,clsExam.showResult, stdExam, exam, clsExam);
                if (examResult != null)
                {

                    return examResult;
                }
                return null;
            }
            catch (InvalidOperationException ex)
            {
                stdExam.Score = 0;
                _studentExamRepo.UpdateEntity(stdExam);
                throw new InvalidOperationException(ex.Message);
            }
        }
        public async Task<ExamResultViewDto> ExamSubmissionPractices(string studentEmail, ExamStudentSubmitDto model)
        {

            var user = await _userManager.FindByEmailAsync(studentEmail);
            var roles = await _userManager.GetRolesAsync(user);

            StudentExam stdExam = await _studentExamRepo.GetByIdAsync(model.stdExamId);


            if (stdExam == null)
            {
                throw new InvalidOperationException($"exam with ID {model.stdExamId} was not found.");
            }
            if (stdExam.isSubmit)
            {
                throw new InvalidOperationException($"you have already submitted the exam.");
            }

            var deliveryTime = DateTime.Now;
            var durationStudent = deliveryTime - stdExam.AttemptStartTime;

            var clsExams = await _classExamRepo.GetAllAsync();
            ClassExam clsExam = clsExams.FirstOrDefault(ce => ce.ExamId == stdExam.clsExamExamId && ce.ClassId == stdExam.clsExamClassId)!;


            try
            {
                stdExam.TimeComplation = durationStudent;
                stdExam.AttemptEndTime = deliveryTime;
                stdExam.isSubmit = true;

                var exam = await _examRepo.GetIncludeById(clsExam.ExamId, "Subject", "ExamQuestions");
                var answers = await _answerRepo.GetAllAsync();
                List<int> answersIDsForStudent = model.questions.Select(q => q.AnswerId).ToList();

                var answersForStudnet = answers.Where(a => answersIDsForStudent.Contains(a.Id));

                decimal markForEachQuestion = 1;
                decimal totalScoreStudent = 0;

                foreach (var ans in answersForStudnet)
                {
                    StudentAnswers stdAnswer = new StudentAnswers
                    {
                        StudentExam = stdExam,
                        SelectAnswer = ans,
                        User = user,
                    };
                    if (ans.IsCorrect == true)
                    {

                        stdAnswer.IsCorrect = true;
                        stdAnswer.Score = markForEachQuestion;
                        totalScoreStudent += markForEachQuestion;
                    }
                    else if (ans.IsCorrect == false)
                    {
                        stdAnswer.IsCorrect = false;
                        stdAnswer.Score = 0;
                    }
                    await _studentAnswerRepo.AddAsyncEntity(stdAnswer);
                }

                stdExam.Score = totalScoreStudent;
                _studentExamRepo.UpdateEntity(stdExam);
                ExamResultViewDto examResult = await getExamResult(false, true, stdExam, exam, clsExam);
                if (examResult != null)
                {

                    return examResult;
                }
                return null;
            }
            catch (InvalidOperationException ex)
            {
                stdExam.Score = 0;
                _studentExamRepo.UpdateEntity(stdExam);
                throw new InvalidOperationException(ex.Message);
            }
        }
        public async Task<bool> enableShowResult(string userEmail,int classId, string examId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);

            Exam exam = await _examRepo.GetByIdAsync(examId);
            Class cls = await _classRepo.GetByIdAsync(classId);



            if (exam == null)
            {
                throw new InvalidOperationException($"Exam with ID {examId} was not found.");
            }
            if(cls == null)
            {
                throw new InvalidOperationException($"Class with ID {classId} was not found.");
            }

            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                //this constrant for create exam ;this all return true or false
                await _examValidation.CheckSubAdminOwnershipClass(user.Id, classId);
                await _examValidation.IsSubjectAssignedToTheClass(exam.SubjectId, classId);

                //this in order checking the exam is published in class
               ClassExam clsExam = await _examValidation.isTheExamPublishedInTheClass(examId, classId, ExamFunctionType.cancelExam);

                clsExam.showResult = true;
                _classExamRepo.UpdateEntity(clsExam);

                return true;
            }

            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                await _examValidation.isTheTeacherLinkedToTheClass(user.Email, classId);
                await _examValidation.IsTheTeacherLinkedToTheSubject(user.Id, exam.SubjectId);
                await _examValidation.IsSubjectAssignedToTheClass(exam.SubjectId, classId);
                //this in order checking the exam is published in class
                ClassExam clsExam = await _examValidation.isTheExamPublishedInTheClass(examId, classId, ExamFunctionType.cancelExam);

                clsExam.showResult = true;
                _classExamRepo.UpdateEntity(clsExam);
                return true;
            }
                return false;
        }
        public async Task<ExamResultViewDto> ViewExamResult(string? userEmail,string studentEmail, int classId, string examId)
        {
            var user = await _userManager.FindByEmailAsync(studentEmail);
            var roles = await _userManager.GetRolesAsync(user);

            Exam exam = await _examRepo.GetIncludeById(examId, "Subject", "ExamQuestions");
            List<StudentExam> stdsExams = await _studentExamRepo.GetAllAsync();
            StudentExam stdExam = stdsExams.FirstOrDefault(se => se.clsExamClassId == classId && se.clsExamExamId == examId && se.userId == user.Id);
            if (stdExam == null)
            {
                throw new InvalidOperationException($"No exam found for the student with ID {user.Id} in class {classId} and exam {examId}.");
            }

           
                
            

            List<ClassExam> clsExams = await _classExamRepo.GetAllAsync();
            ClassExam clsExam = clsExams.FirstOrDefault(ce => ce.ExamId == examId && ce.ClassId == classId);
            ExamResultViewDto examResult;
            if (userEmail ==null)
            {
                if (stdExam.isSubmit == false)
                {
                    throw new InvalidOperationException("The student has not submitted the exam yet.");
                }
                examResult =  await getExamResult(false,clsExam.showResult, stdExam, exam, clsExam);
            }
            examResult = await getExamResult(true, clsExam.showResult, stdExam, exam, clsExam);


            return examResult;

        }
        public async Task<List<ExamPreviousViewDto>> GetExamPrevious(string studentEmail, int classId)
        {
            var user = await _userManager.FindByEmailAsync(studentEmail);
            //validate
            await _examValidation.isTheStudentLinkedToTheClass(studentEmail, classId);
            var allStdExams = await _studentExamRepo.GetAllIncludeAsync("clsExam");
            var stdExams = allStdExams.Where(se => se.userId == user.Id && se.clsExamClassId == classId);
            Class cls = await _classRepo.GetIncludeById(classId, "Subject");
            if (stdExams.Count() == 0)
            {
                throw new InvalidOperationException("Not Found Any Exam");
            }

            List<ExamPreviousViewDto> examsPrevious = new List<ExamPreviousViewDto>();
            foreach (var stdExm in stdExams.Where(se => se.clsExam.showResult && se.isSubmit))
            {
                Exam exam = await _examRepo.GetIncludeById(stdExm.clsExamExamId, "Subject");
                ExamPreviousViewDto exprevs = new ExamPreviousViewDto
                {
                    examId = stdExm.clsExamExamId,
                    ExamType = stdExm.ExamType,
                    Title = exam.Title,
                    Description = exam.Description,
                    SubjectName = exam.Subject.Name,
                    scoreExam = stdExm.clsExam.Score,
                    ScoreStudent = stdExm.Score,
                    AttemptStartTime = stdExm.AttemptStartTime,
                    AttemptEndTime = stdExm.AttemptEndTime,
                    TimeComplation = stdExm.TimeComplation,
                };
                examsPrevious.Add(exprevs);
            }

            return examsPrevious;
        }
        public async Task<List<AttendenceViewDto>> DisplayExamAttendenceAndResults(string userEmail, int classId, string examId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);

            List<StudentExam> allStdsEams = await _studentExamRepo.GetAllIncludeAsync("User");
            List<AttendenceViewDto> stdsExamsInClass = allStdsEams.Where(se => se.clsExamClassId == classId && se.clsExamExamId == examId)
                .Select(se => new AttendenceViewDto
                {
                    examId = se.clsExamExamId,
                    classId = se.clsExamClassId,
                    studentEamil = se.User.Email,
                    studentName = $"{se.User.FirstName} {se.User.LastName}",
                    score = se.Score
                }).ToList();

            try
            {

                if (roles.Contains(Roles.SubAdmin.ToString()))
                {
                    await _examValidation.CheckSubAdminOwnershipClass(user.Id, classId);
                    return stdsExamsInClass;

                }
                else if (roles.Contains(Roles.Teacher.ToString()))
                {

                    await _examValidation.isTheTeacherLinkedToTheClass(user.Email, classId);
                    return stdsExamsInClass;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<ExamViewInSubjecDto>> GetAllExams(string userEmail, int subjectId, int departmentId)
        {
            List<ExamViewInSubjecDto> exams = new List<ExamViewInSubjecDto>();
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            Subject subject = await _subjectRepo.GetIncludeById(subjectId, "Exams");
            if (subject == null)
            {
                throw new InvalidOperationException($"subject with ID {subjectId} was not found.");
            }
            await _examValidation.IsSubjectAssignedToDepartment(subjectId, departmentId);

            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                await _examValidation.CheckSubAdminOwnershipDepartment(userEmail, departmentId);
                exams = await getAllExamsInSubject(subject.Exams.ToList());

            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {

                await _examValidation.IsTheTeacherLinkedToTheSubject(user.Id, subjectId);
                exams = await getAllExamsInSubject(subject.Exams.ToList());
            }

            return exams;
        }
        public async Task<ExamViewDto> GetExamById(string userEmail, string examId, int departmentId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            Exam exam = await _examRepo.GetByIdAsync(examId);
            if (exam == null)
            {
                throw new InvalidOperationException($"exam with ID {examId} was not found.");
            }
            await _examValidation.IsSubjectAssignedToDepartment(exam.SubjectId, departmentId);

            var examQuestions = await _examQuestionRepo.GetAllIncludeAsync("Question");

            var questions = examQuestions.Where(ex => ex.ExamId == exam.Id).Select(exQu => exQu.Question).ToList();

            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                await _examValidation.CheckSubAdminOwnershipDepartment(userEmail, departmentId);
                return await getExam(exam, questions);

            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {

                await _examValidation.IsTheTeacherLinkedToTheSubject(user.Id, exam.SubjectId);
                return await getExam(exam, questions);
            }
            return null;
        }

        public async Task<List<ExamAvalibleDto>> GetExamsAvalibie(string userEmail, int classId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            Class cls = await _classRepo.GetIncludeById(classId, "Subject");
            if (cls == null)
            {
                throw new InvalidOperationException($"class with ID {classId} was not found.");
            }

            List<ExamAvalibleDto> avalibleExams = new List<ExamAvalibleDto>();




            var classExams = await _classExamRepo.GetAllIncludeAsync("Exam", "StudentExam");

            var exs = classExams
                .Where(cls => cls.ClassId == classId && cls.EndTime >= DateTime.Now)
                .Where(cls => !cls.StudentExam.Any(stdExam => stdExam.userId == user.Id))
                .Select(cls => new
                {
                    exam = cls.Exam,
                    score = cls.Score,
                    duration = cls.Duration,
                    startExam = cls.StartTime,
                    endExam = cls.EndTime
                })
                .ToList();


            foreach (var ex in exs)
            {
                AppUser userCreateExam = await _userManager.FindByIdAsync(ex.exam.UserId);
                ExamAvalibleDto aviExam = new ExamAvalibleDto
                {
                    Id = ex.exam.Id,
                    Title = ex.exam.Title,
                    Description = ex.exam.Description,
                    score = ex.score,
                    duration = ex.duration,
                    subjectName = cls.Subject.Name,
                    startExame = ex.startExam,
                    endExame = ex.endExam,
                    userName = userCreateExam.UserName
                };
                avalibleExams.Add(aviExam);
            }




            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                await _examValidation.CheckSubAdminOwnershipClass(user.Id, classId);
                if (exs.Count == 0)
                {
                    throw new InvalidOperationException("There are no available exams currently for this class.");
                }
                return avalibleExams;

            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                await _examValidation.isTheTeacherLinkedToTheClass(userEmail, classId);
                if (exs.Count == 0)
                {
                    throw new InvalidOperationException("There are no available exams currently for this class.");
                }
                return avalibleExams;
            }
            else if (roles.Contains(Roles.Student.ToString()))
            {
                await _examValidation.isTheStudentLinkedToTheClass(userEmail, classId);
                if (exs.Count == 0)
                {
                    throw new InvalidOperationException("There are no available exams currently for this class.");
                }
                return avalibleExams;
            }
            return null;
        }
        public async Task<ExamCounterViewDto> GetExamCounterForStudent(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            List<StudentClass> studenstClss = await _studentClassRepo.GetAllIncludeAsync("Class");
            List<Class> studentClasses = studenstClss.Where(sc=> sc.UserId == user.Id).Select(s=> s.Class).ToList();
            
      
            int examAvalibaleCount = 0;
            int previousExamCount = 0;


            foreach(Class cls in studentClasses)
            {

            List<ClassExam> classExams = await _classExamRepo.GetAllIncludeAsync("StudentExam");
                int exsAvalibleCount = classExams
                    .Where(c => c.ClassId == cls.Id && c.EndTime >= DateTime.Now).Count(cls => !cls.StudentExam.Any(stdExam => stdExam.userId == user.Id));
                examAvalibaleCount += exsAvalibleCount;

                int exsPrevieousCount = classExams
                            .Where(c => c.ClassId == cls.Id).Count(cls => cls.StudentExam.Any(stdExam => stdExam.userId == user.Id));
            
                previousExamCount += exsPrevieousCount;
            }


             if (roles.Contains(Roles.Student.ToString()))
            {


                return new ExamCounterViewDto
                {
                    examAvalibaleCounter = examAvalibaleCount,
                    previousExamCounter = previousExamCount,
                };
            }
            return null;
         
        }

        public async Task<ExamStudentViewDto> GetExamPractices(string studentEmail, int classId, List<int> learningOutcomeIds, int questionCount)
        {
            var user = await _userManager.FindByEmailAsync(studentEmail);
            Class cls = await _classRepo.GetByIdAsync(classId);

            if (cls == null)
            {
                throw new ArgumentException($"Class with ID {classId} was not found.");
            }

            //###### validation
            if (cls.SubjectId == null)
            {
                throw new InvalidOperationException("There is no subject associated with this class.");
            }

            await _examValidation.isTheStudentLinkedToTheClass(user.Email, classId);
            await _examValidation.IsLearningOutcomesFounded(cls.SubjectId.Value, learningOutcomeIds);

            ExamStudentViewDto exStudentView = await generateExamPractices(user, classId, cls.SubjectId.Value, learningOutcomeIds, questionCount);

            return exStudentView;

        }
        public async Task<List<ExamAvalibleDto>> getExamPublishedInClass(string userEmail, int classId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            Class cls = await _classRepo.GetIncludeById(classId, "Subject");
            if (cls == null)
            {
                throw new InvalidOperationException($"class with ID {classId} was not found.");
            }

            List<ExamAvalibleDto> avalibleExams = new List<ExamAvalibleDto>();

            var classExams = await _classExamRepo.GetAllIncludeAsync("Exam");
            var exs = classExams.Where(cls => cls.ClassId == classId).Select(cls => new
            {
                exam = cls.Exam,
                score = cls.Score,
                duration = cls.Duration,
                startExam = cls.StartTime,
                endExam = cls.EndTime,
                isShowResult = cls.showResult
            }).ToList();

            foreach (var ex in exs)
            {
                AppUser userCreateExam = await _userManager.FindByIdAsync(ex.exam.UserId);
                ExamAvalibleDto aviExam = new ExamAvalibleDto
                {
                    Id = ex.exam.Id,
                    Title = ex.exam.Title,
                    Description = ex.exam.Description,
                    score = ex.score,
                    duration = ex.duration,
                    subjectName = cls.Subject.Name,
                    startExame = ex.startExam,
                    endExame = ex.endExam,
                    isShowResult = ex.isShowResult,
                    userName = userCreateExam.UserName
                };
                avalibleExams.Add(aviExam);
            }




            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                await _examValidation.CheckSubAdminOwnershipClass(user.Id, classId);
                if (exs.Count == 0)
                {
                    throw new InvalidOperationException("There are no exams currently for this class.");
                }
                return avalibleExams;

            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                await _examValidation.isTheTeacherLinkedToTheClass(userEmail, classId);
                if (exs.Count == 0)
                {
                    throw new InvalidOperationException("There are no exams currently for this class.");
                }
                return avalibleExams;
            }
            else if (roles.Contains(Roles.Student.ToString()))
            {
                await _examValidation.isTheStudentLinkedToTheClass(userEmail, classId);
                if (exs.Count == 0)
                {
                    throw new InvalidOperationException("There are no exams currently for this class.");
                }
                return avalibleExams;
            }
            return null;
        }
        public async Task<List<ExamAvalibleDto>> getCompletedExams(string userEmail,int classId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            Class cls = await _classRepo.GetIncludeById(classId, "Subject");
            if (cls == null)
            {
                throw new InvalidOperationException($"class with ID {classId} was not found.");
            }

            List<ExamAvalibleDto> avalibleExams = new List<ExamAvalibleDto>();
            var classExams = await _classExamRepo.GetAllIncludeAsync("Exam", "StudentExam");

            var exs = classExams
                .Where(cls => cls.ClassId == classId && cls.EndTime < DateTime.Now)
                .Select(cls => new
                {
                    exam = cls.Exam,
                    score = cls.Score,
                    duration = cls.Duration,
                    startExam = cls.StartTime,
                    endExam = cls.EndTime
                })
                .ToList();


            foreach (var ex in exs)
            {
                AppUser userCreateExam = await _userManager.FindByIdAsync(ex.exam.UserId);
                ExamAvalibleDto aviExam = new ExamAvalibleDto
                {
                    Id = ex.exam.Id,
                    Title = ex.exam.Title,
                    Description = ex.exam.Description,
                    score = ex.score,
                    duration = ex.duration,
                    subjectName = cls.Subject.Name,
                    startExame = ex.startExam,
                    endExame = ex.endExam,
                    userName = userCreateExam.UserName
                };
                avalibleExams.Add(aviExam);
            }




            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                await _examValidation.CheckSubAdminOwnershipClass(user.Id, classId);
                if (exs.Count == 0)
                {
                    throw new InvalidOperationException("There are no available exams currently for this class.");
                }
                return avalibleExams;

            }
            return null;
        }



        // ################################# this function resued ################################################################
        private async Task<ExamViewDto> generateExam(ExamCreateDto model, Subject subject, AppUser user)
        {
            var initQS = await initQuestion(model.learningOutcomeIds, model.NumberOfEasyQuestions, model.NumberOfMediumLevelQuestions, model.NumberOfDifficultQuestions, model.ClarityRangeFrom, model.ClarityRangeTo);

            var questionCount = initQS.Count();
            Exam exam = new Exam
            {
                Id = Guid.NewGuid().ToString(),
                Title = model.Title,
                Description = model.Description,
                Subject = subject,
                CreatedDate = DateTime.Now,
                NumberOfEasyQuestions = model.NumberOfEasyQuestions,
                NumberOfMediumLevelQuestions = model.NumberOfMediumLevelQuestions,
                NumberOfDifficultQuestions = model.NumberOfDifficultQuestions,
                ClarityRangeFrom = model.ClarityRangeFrom,
                ClarityRangeTo = model.ClarityRangeTo,
                questionCount = questionCount,
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
            List<LearningOutcomes> learningOutcomes = await getLearningOutcomesUsingQuestions(initQS);
            return new ExamViewDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                CreatedDate = exam.CreatedDate,
                userName = user.Email,
                Subject = subject.Name,
                learningOutComes = learningOutcomes.Select(lrn => lrn.Title).ToList(),
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
        private async Task<List<Question>> initQuestion(List<int> learningOutComesIDs, int easyCount, int mediumCount, int hardCount, decimal clarityRangeFrom, decimal clarityRangeTo)
        {
            // # step-1
            var questionByLearningOutComes = FilterQuestionsByLearningOutComes(learningOutComesIDs);

            var questionsInRange = FilterQuestionsByClarityRange(questionByLearningOutComes, clarityRangeFrom, clarityRangeTo);

            if (!questionsInRange.Any())
            {
                throw new InvalidOperationException("No questions found in the specified clarity range.");
            }

            var easyQuestions = FilterByDifficulty(questionsInRange, 0, 0.4m);
            var mediumQuestions = FilterByDifficulty(questionsInRange, 0.4m, 0.7m);
            var hardQuestions = FilterByDifficulty(questionsInRange, 0.7m, 0.1m);

            ValidateAvailability(easyQuestions, easyCount, "easy");
            ValidateAvailability(mediumQuestions, mediumCount, "medium");
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

            return await ShuffleQuestions(finalExamQuestions);
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
        private async Task<List<Question>> ShuffleQuestions(List<Question> questions)
        {
            var random = new Random();

            var shuffledIds = questions
                .OrderBy(q => random.Next())
                .Select(q => q.Id)
                .ToList();

            var questionsWithAnswer = await _questionRepo.GetAllIncludeAsync("Answers");

            var questionsWithAnswers = questionsWithAnswer
                .Where(q => shuffledIds.Contains(q.Id))
                .ToList();

            foreach (var question in questionsWithAnswers)
            {
                question.Answers = question.Answers
                    .OrderBy(a => random.Next())
                    .ToList();
            }

            return shuffledIds
                .Select(id => questionsWithAnswers.First(q => q.Id == id))
                .ToList();
        }



        private void ValidateAvailability(List<Question> questions, int requiredCount, string difficulty)
        {
            if (questions.Count < requiredCount)
                throw new InvalidOperationException($"Not enough {difficulty} questions available.");
        }
        private async Task<List<ExamViewInSubjecDto>> getAllExamsInSubject(List<Exam> exs)
        {
            if (exs.Count == 0)
            {
                throw new InvalidOperationException("No exams found for this subject.");
            }
            List<ExamViewInSubjecDto> exams = new List<ExamViewInSubjecDto>();
            foreach (Exam ex in exs)
            {
                var userExam = await _userManager.FindByIdAsync(ex.UserId);

                var exam = new ExamViewInSubjecDto
                {
                    Id = ex.Id,
                    Title = ex.Title,
                    Description = ex.Description,
                    NumberOfEasyQuestions = ex.NumberOfEasyQuestions,
                    NumberOfMediumLevelQuestions = ex.NumberOfMediumLevelQuestions,
                    NumberOfDifficultQuestions = ex.NumberOfDifficultQuestions,
                    ClarityRangeFrom = ex.ClarityRangeFrom,
                    ClarityRangeTo = ex.ClarityRangeTo,
                    CreatedDate = ex.CreatedDate,
                    userName = userExam.UserName,
                };
                exams.Add(exam);
            }
            return exams;
        }
        private async Task<ExamViewDto> getExam(Exam exam, List<Question> qts)
        {
            var user = await _userManager.FindByIdAsync(exam.UserId);
            var subject = await _subjectRepo.GetByIdAsync(exam.SubjectId);
            var learningOutComes = await getLearningOutcomesUsingQuestions(qts);

            var qs = await _questionRepo.GetAllIncludeAsync("Answers");
            var questions = qs.Where(q => qts.Contains(q));
            ;
            return new ExamViewDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                CreatedDate = exam.CreatedDate,
                userName = user.UserName,
                Subject = subject.Name,
                learningOutComes = learningOutComes.Select(l => l.Title).ToList(),
                questions = questions.Select(q => new QuestionViewDto
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
        private async Task<ExamStudentViewDto> getExamForStudent(string stdExamId, Exam exam, List<Question> qts, ClassExam clsExam)
        {
            var user = await _userManager.FindByIdAsync(exam.UserId);
            var subject = await _subjectRepo.GetByIdAsync(exam.SubjectId);

            var qs = await _questionRepo.GetAllIncludeAsync();
            var questionsBeForShuffle = qs.Where(q => qts.Contains(q)).ToList();
            List<Question> questions = await ShuffleQuestions(questionsBeForShuffle);


            if (clsExam != null)
            {

                return new ExamStudentViewDto
                {
                    stdExamId = stdExamId,
                    Title = exam.Title,
                    Description = exam.Description,
                    SubjectName = subject.Name,
                    Score = clsExam.Score,
                    Duration = clsExam.Duration,
                    questions = questions.Select(q => new QuestionExamStudentViewDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.QuestionText,
                        answers = q.Answers.Select(ans => new AnswerExamStudentView
                        {
                            Id = ans.Id,
                            AnswerText = ans.AnswerText,
                        }).ToList()
                    }).ToList()
                };

            }
            else
            {

                return new ExamStudentViewDto
                {
                    stdExamId = stdExamId,
                    Title = exam.Title,
                    Description = exam.Description,
                    SubjectName = subject.Name,
                    questions = questions.Select(q => new QuestionExamStudentViewDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.QuestionText,
                        answers = q.Answers.Select(ans => new AnswerExamStudentView
                        {
                            Id = ans.Id,
                            AnswerText = ans.AnswerText,
                        }).ToList()
                    }).ToList()
                };
            }
        }
        private async Task<List<LearningOutcomes>> getLearningOutcomesUsingQuestions(List<Question> questions)
        {
            List<int> learningOutComesIDs = new List<int>();
            foreach (var q in questions)
            {
                learningOutComesIDs.Add(q.learningOutComesId);
            }
            learningOutComesIDs = learningOutComesIDs.Distinct().ToList();

            List<LearningOutcomes> lrnOut = await _learningOutComesRepo.GetAllAsync();
            List<LearningOutcomes> learningOutComes = lrnOut.Where(lrn => learningOutComesIDs.Contains(lrn.Id)).ToList();
            return learningOutComes;

        }


        private async Task<ExamResultViewDto> getExamResult(bool isTeacher, bool isShowResult, StudentExam stdExam, Exam exam, ClassExam clsExam)
        {
            if (isShowResult || isTeacher)
            {
                var allStdAnswers = await _studentAnswerRepo.GetAllIncludeAsync("SelectAnswer");
                var stdAnswers = allStdAnswers
                    .Where(ans => ans.studentExamId == stdExam.id)
                    .ToList();

                var questionIds = exam.ExamQuestions.Select(eq => eq.QuestionId).ToList();
                var allQuestions = await _questionRepo.GetAllIncludeAsync("Answers");

                var questions = allQuestions
                    .Where(q => questionIds.Contains(q.Id))
                    .Select(q => new
                    {
                        QuestionId = q.Id,
                        q.QuestionText,
                        CorrectAnswer = q.Answers.FirstOrDefault(a => a.IsCorrect)
                    })
                    .Where(q => q.CorrectAnswer != null) // تأكد أن هناك إجابة صحيحة
                    .ToList();

                var questionResultViewDtos = questions.Select(q =>
                {
                    var selectedAnswer = stdAnswers
                        .FirstOrDefault(ans => ans.SelectAnswer.QuestionId == q.QuestionId)?.SelectAnswer;

                    return new QuestionResultViewDto
                    {
                        questionText = q.QuestionText,
                        correctAnswerId = q.CorrectAnswer.Id,
                        correctAnswer = q.CorrectAnswer.AnswerText,
                        selectAnswerId = selectedAnswer?.Id,
                        selectAnswer = selectedAnswer?.AnswerText
                    };
                }).ToList();

                var examResult = new ExamResultViewDto
                {
                    stdExamId = stdExam.id,
                    Title = exam.Title,
                    Description = exam.Description,
                    SubjectName = exam.Subject?.Name,
                    Score = clsExam.Score,
                    StudentScore = stdExam.Score,
                    AttemptStartTime = stdExam.AttemptStartTime,
                    AttemptEndTime = stdExam.AttemptEndTime,
                    TimeComplation = stdExam.TimeComplation,
                    questionResultViewDtos = questionResultViewDtos
                };

                return examResult;
            }

            return null;

        }


        private async Task<ExamStudentViewDto> generateExamPractices(AppUser user, int classId, int subjectId, List<int> learningOutComesIDs, int questionCount)
        {
            var questionByLearningOutComes = FilterQuestionsByLearningOutComes(learningOutComesIDs);
            if (questionByLearningOutComes.Count() < questionCount)
            {
                throw new InvalidOperationException("Not enough questions available for the specified learning outcome.");
            }

            var random = new Random();
            List<Question> selectQuestions = questionByLearningOutComes.OrderBy(q => random.Next()).Take(questionCount).ToList();

            Exam exam = new Exam
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Practices Exam",
                Description = "This exam is to test and train the student's ability.",
                CreatedDate = DateTime.Now,
                AppUser = user,
                questionCount = questionCount,
                SubjectId = subjectId
            };
            await _examRepo.AddAsyncEntity(exam);

            foreach (var q in selectQuestions)
            {
                ExamQuestion examQuestion = new ExamQuestion
                {
                    Exam = exam,
                    Question = q
                };
                await _examQuestionRepo.AddAsyncEntity(examQuestion);
            }

            ClassExam clsExam = new ClassExam
            {
                Exam = exam,
                ClassId = classId,
                Score = questionCount,
                showResult = true,

            };

            await _classExamRepo.AddAsyncEntity(clsExam);


            StudentExam stdExam = new StudentExam
            {
                id = Guid.NewGuid().ToString(),
                clsExamClassId = classId,
                clsExamExamId = exam.Id,
                Score = questionCount,
                AttemptStartTime = DateTime.Now,
                ExamType = ExamType.Practice.ToString(),
                User = user,
            };

            await _studentExamRepo.AddAsyncEntity(stdExam);
            ExamStudentViewDto exStudentView = await getExamForStudent(stdExam.id, exam, selectQuestions, null);
            return exStudentView;
        }
    }
}
