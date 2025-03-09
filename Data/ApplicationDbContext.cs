using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using QuizHub.Models;
using System.Reflection.Emit;

namespace QuizHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }


        public DbSet<College> Colleges { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Batch> Batchs { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<LearningOutcomes> LearingOutcomes { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<ClassExam> ClassExams { get; set; }
        public DbSet<StudentExam> StudentExams { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //for composite key

            builder.Entity<UserDepartment>()
                .HasKey(ud => new { ud.userId, ud.departmentId });
            builder.Entity<UserDepartment>()
           .HasOne(ud => ud.User)
           .WithMany(u => u.userDepartments)
           .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<UserDepartment>()
                .HasOne(ud => ud.Department)
                .WithMany(d => d.UserDepartments)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentClass>()
                .HasKey(sc => new { sc.UserId, sc.ClassId });
            builder.Entity<StudentClass>()
            .HasOne(sc => sc.User)
            .WithMany(u => u.StudentClasses)
            .HasForeignKey(sc => sc.UserId)
            .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<StudentClass>()
                .HasOne(sc => sc.Class)
                .WithMany(c => c.StudentClasses)
                .HasForeignKey(sc => sc.ClassId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<StudentExam>()
                .HasKey(se => new { se.userId, se.examId });
            builder.Entity<ExamQuestion>()
                .HasKey(eq => new { eq.ExamId, eq.QuestionId });
            builder.Entity<ClassExam>()
                .HasKey(ce => new { ce.ClassId, ce.ExamId });

            // Student Answer
            builder.Entity<StudentAnswer>()
                .HasKey(sa => new { sa.userId, sa.QuestionId });
            builder.Entity<StudentAnswer>()
                .HasOne(sa => sa.User)
                .WithMany(u => u.StudentAnswers)
                .HasForeignKey(sa => sa.userId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<StudentAnswer>()
                .HasOne(sa => sa.Question)
                .WithMany(q => q.StudentAnswers)
                .HasForeignKey(sa => sa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<StudentAnswer>()
                .HasOne(sa => sa.Exam)
                .WithMany(e => e.studentAnswers)
                .HasForeignKey(sa => sa.ExamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.userId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.Class)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.classId)
                .OnDelete(DeleteBehavior.Restrict);


        }

    }
}
