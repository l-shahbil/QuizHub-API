using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Exam;
using QuizHub.Services.Shared_Services;
using QuizHub.Services.Shared_Services.Interface;
using QuizHub.Services.SubAdmin_Services.Interface;

namespace QuizHub.Services.SubAdmin_Services
{
    public class ReportService:IReportService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<ClassExam> _classExam;
        private readonly IExamValidator _examValidator;
        private readonly IRepository<Class> _classRepo;

        public ReportService(UserManager<AppUser> userManager,IRepository<ClassExam> classExam,IExamValidator examValidator, IRepository<Class> classRepo)
        {
            _userManager = userManager;
            _classExam = classExam;
            _examValidator = examValidator;
            _classRepo = classRepo;
        }

        public async Task<ExamResultReportDto> getReportExam(string userEmail, int classId, string examId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            List<ClassExam> clsExams = await _classExam.GetAllIncludeAsync("Exam", "StudentExam");
            ClassExam clsExam = clsExams.FirstOrDefault(clsEx => clsEx.ExamId == examId && clsEx.ClassId == classId);

            if (clsExam == null)
            {
                throw new KeyNotFoundException($"class with ID {classId} was not found.");
            }

            await _examValidator.CheckSubAdminOwnershipClass(user.Id, classId);

            ClassExam clsEx = await _classExam.GetFirstOrDefaultAsync(
                filter: ce=> ce.ClassId ==classId && ce.ExamId == examId,
                include:query=> query.Include(ce=> ce.Class).ThenInclude(c=>c.StudentClasses)
                .Include(clsExam => clsExam.StudentExam)
                );

           List<StudentExam> StudentsWhoAttended = clsEx.StudentExam.ToList();



            int clsStudentCount = clsEx.Class.StudentClasses.Count();
            int StudentsWhoAttendedCount = StudentsWhoAttended.Count();
            int StudentsWhoDidNotAttendCount = clsStudentCount - StudentsWhoAttendedCount;
                decimal totalStudentScore = StudentsWhoAttended.Sum(se => se.Score);
            decimal averageScore = 0;
            decimal passRate = 0;
            decimal passMark = clsEx.Score / 2;
            int numberOfStudentsEWoPassed = clsEx.StudentExam.Count(se => se.Score >= passMark);
            decimal lowestScore = 0;
            decimal topScore = 0;

            if (clsEx.StudentExam.Count() != 0)
            {
             topScore = clsEx.StudentExam.Max(se => se.Score);
             lowestScore= clsEx.StudentExam.Min(se => se.Score);
            }

            if (clsStudentCount != 0)
            {
                averageScore = totalStudentScore / clsStudentCount;

            }
            if (StudentsWhoAttendedCount != 0)
            {
                passRate =(decimal)numberOfStudentsEWoPassed / StudentsWhoAttendedCount;

            }
      

            ExamResultReportDto exmReport = new ExamResultReportDto
            {
                TotalStudentsInClass = clsStudentCount,
                StudentsWhoAttended =StudentsWhoAttendedCount,
                StudentsWhoDidNotAttend =StudentsWhoDidNotAttendCount,
                AverageScore = averageScore,
                PassRate = $"%{100* passRate}",
                TopScore =topScore,
                LowestScore = lowestScore

            };

            return exmReport;
        }
    }
}
