namespace LearningSystem.Data
{
    using LearningSystem.Data.Models;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class LearningSystemDbContext : IdentityDbContext<User>
    {
        public LearningSystemDbContext(DbContextOptions<LearningSystemDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<ExamSubmission> ExamSubmissions { get; set; }

        public DbSet<Certificate> Certificates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Many-to-Many
            builder
                .Entity<StudentCourse>()
                .HasKey(sc => new { sc.StudentId, sc.CourseId });

            builder
                .Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.Courses)
                .HasForeignKey(sc => sc.StudentId);

            builder
               .Entity<StudentCourse>()
               .HasOne(sc => sc.Course)
               .WithMany(c => c.Students)
               .HasForeignKey(sc => sc.CourseId);

            // One-to-Many
            builder
               .Entity<Course>()
               .HasOne(c => c.Trainer)
               .WithMany(t => t.Trainings)
               .HasForeignKey(c => c.TrainerId)
               .OnDelete(DeleteBehavior.Restrict); // Required Trainer

            builder
                .Entity<Article>()
                .HasOne(a => a.Author)
                .WithMany(a => a.Articles)
                .HasForeignKey(a => a.AuthorId);

            builder
                .Entity<ExamSubmission>()
                .HasOne(e => e.Course)
                .WithMany(c => c.ExamSubmissions)
                .HasForeignKey(e => e.CourseId);

            builder
                .Entity<ExamSubmission>()
                .HasOne(e => e.Student)
                .WithMany(st => st.ExamSubmissions)
                .HasForeignKey(e => e.StudentId);

            builder
                .Entity<Certificate>()
                .HasOne(c => c.Course)
                .WithMany(c => c.Certificates)
                .HasForeignKey(c => c.CourseId);

            builder
                .Entity<Certificate>()
                .HasOne(c => c.Student)
                .WithMany(st => st.Certificates)
                .HasForeignKey(c => c.StudentId);
        }
    }
}
