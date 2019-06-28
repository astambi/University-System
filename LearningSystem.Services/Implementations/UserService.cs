namespace LearningSystem.Services.Implementations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using Microsoft.EntityFrameworkCore;

    public class UserService : IUserService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public UserService(
            LearningSystemDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<bool> CanBeDeleted(string id)
            => !await this.db.Courses.AnyAsync(c => c.TrainerId == id)
            && !await this.db.Articles.AnyAsync(a => a.AuthorId == id);

        public async Task<CertificateServiceModel> GetCertificateDataAsync(string id)
            => await this.db
            .Certificates
            .Where(c => c.Id == id)
            .Select(c => new CertificateServiceModel
            {
                Id = id,
                Course = c.Course.Name,
                StartDate = c.Course.StartDate,
                EndDate = c.Course.EndDate,
                Student = c.Student.Name,
                Grade = c.Grade,
                Trainer = c.Course.Trainer.Name,
                IssueDate = c.IssueDate
            })
            .FirstOrDefaultAsync();

        public async Task<UserEditServiceModel> GetProfileToEditAsync(string id)
            => await this.db
            .Users
            .Where(u => u.Id == id)
            .Select(u => this.mapper.Map<UserEditServiceModel>(u))
            .FirstOrDefaultAsync();

        public async Task<UserProfileServiceModel> GetUserProfileAsync(string id)
            => await this.db
            .Users
            .Where(u => u.Id == id)
            .Select(u => new UserProfileServiceModel
            {
                User = this.mapper.Map<UserWithBirthdateServiceModel>(u),
                Courses = u.Courses
                    //.Select(sc => this.MapCourseWithGrade(sc, sc.Course))
                    .Select(sc => new CourseProfileServiceModel
                    {
                        Id = sc.CourseId,
                        Name = sc.Course.Name,
                        Grade = sc.Grade,
                        StartDate = sc.Course.StartDate,
                        EndDate = sc.Course.EndDate,
                        CertificateId = sc.Course
                            .Certificates
                            .Where(c => c.StudentId == sc.StudentId)
                            .OrderBy(c => c.Grade)
                            .Select(c => c.Id)
                            .FirstOrDefault()
                    })
                    .OrderByDescending(c => c.StartDate)
                    .ThenByDescending(c => c.EndDate)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        public async Task UpdateUserProfileAsync(string id, string name, DateTime birthdate)
        {
            var user = await this.db.Users.FindAsync(id);
            if (user == null)
            {
                return;
            }

            user.Name = name;
            user.Birthdate = birthdate;

            await this.db.SaveChangesAsync();
        }

        private CourseProfileServiceModel MapCourseWithGrade(StudentCourse studentCourse, Course course)
        {
            var courseDto = this.mapper.Map<CourseProfileServiceModel>(course);
            courseDto.Grade = studentCourse.Grade;
            courseDto.CertificateId = studentCourse
                .Course?
                .Certificates
                .Where(c => c.StudentId == studentCourse.StudentId)
                .OrderBy(c => c.Grade)
                .Select(c => c.Id)
                .FirstOrDefault();

            return courseDto;
        }
    }
}
