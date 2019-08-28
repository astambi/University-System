namespace University.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using University.Data.Models;

    public class UniversityDbContext : IdentityDbContext<User>
    {
        public UniversityDbContext(DbContextOptions<UniversityDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<ExamSubmission> ExamSubmissions { get; set; }

        public DbSet<Certificate> Certificates { get; set; }

        public DbSet<Resource> Resources { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Curriculum> Curriculums { get; set; }

        public DbSet<Diploma> Diplomas { get; set; }

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

            builder
               .Entity<CurriculumCourse>()
               .HasKey(cc => new { cc.CurriculumId, cc.CourseId });

            builder
                .Entity<CurriculumCourse>()
                .HasOne(cc => cc.Curriculum)
                .WithMany(c => c.Courses)
                .HasForeignKey(cc => cc.CurriculumId);

            builder
                .Entity<CurriculumCourse>()
                .HasOne(cc => cc.Course)
                .WithMany(c => c.Curriculums)
                .HasForeignKey(cc => cc.CourseId);

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

            builder
                .Entity<Resource>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Resources)
                .HasForeignKey(r => r.CourseId);

            //// Removing FK Orders_Users_UserId to allow users to delete their profiles without affecting the payments history
            builder
                .Entity<Order>()
                .HasIndex(o => o.UserId);

            builder
                .Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);

            builder
                .Entity<Diploma>()
                .HasOne(d => d.Curriculum)
                .WithMany(c => c.Diplomas)
                .HasForeignKey(d => d.CurriculumId);

            builder
                .Entity<Diploma>()
                .HasOne(d => d.Student)
                .WithMany(st => st.Diplomas)
                .HasForeignKey(d => d.StudentId);
        }
    }
}
