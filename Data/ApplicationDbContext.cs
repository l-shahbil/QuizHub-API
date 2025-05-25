
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
        public DbSet<StudentAnswers> StudentAnswers { get; set; }
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
                .OnDelete(DeleteBehavior.Cascade);

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
      .HasOne(se => se.clsExam)
      .WithMany(ce => ce.StudentExam)
      .HasForeignKey(se => new { se.clsExamClassId, se.clsExamExamId })
      .HasPrincipalKey(ce => new { ce.ClassId, ce.ExamId })
      .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamQuestion>()
                .HasKey(eq => new { eq.ExamId, eq.QuestionId });

            // Student Answer
            builder.Entity<StudentAnswers>()
                .HasOne(sa => sa.User)
                .WithMany(u => u.StudentAnswers)
                .HasForeignKey(sa => sa.userId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAnswers>()
.HasOne(se => se.StudentExam)
.WithMany(ce => ce.studentAnswers)
.HasForeignKey(se => new { se.studentExamId})
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

            builder.Entity<ClassExam>()
    .HasKey(ce => new { ce.ClassId, ce.ExamId });
            builder.Entity<ClassExam>()
    .HasOne(ce => ce.Exam)
    .WithMany(e => e.classExams)
    .HasForeignKey(ce => ce.ExamId)
    .OnDelete(DeleteBehavior.Restrict);
 
            builder.Entity<Question>()
                .HasOne(q=> q.leraningOutComes)
                .WithMany(lrn => lrn.Questions)
                .HasForeignKey(q=> q.learningOutComesId)
                .OnDelete(DeleteBehavior.Restrict);



            builder.Entity<Class>()
    .HasOne(c => c.Subject)
    .WithMany(s => s.Classes)
    .HasForeignKey(c => c.SubjectId)
    .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
