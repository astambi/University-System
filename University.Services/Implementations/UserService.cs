namespace University.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Data;
    using University.Data.Models;
    using University.Services.Models.Certificates;
    using University.Services.Models.Courses;
    using University.Services.Models.Exams;
    using University.Services.Models.Resources;
    using University.Services.Models.Users;

    public class UserService : IUserService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public UserService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<bool> CanBeDeletedAsync(string id)
            => !await this.db.Courses.AnyAsync(c => c.TrainerId == id)
            && !await this.db.Articles.AnyAsync(a => a.AuthorId == id);

        public async Task<UserEditServiceModel> GetProfileToEditAsync(string id)
            => await this.GetUserById(id)
            .ProjectTo<UserEditServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public async Task<UserProfileServiceModel> GetProfileAsync(string id)
            => await this.GetUserById(id)
            .ProjectTo<UserProfileServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public async Task<IEnumerable<CourseProfileMaxGradeServiceModel>> GetCoursesAsync(string id)
            => await this.GetUserById(id)
            .SelectMany(u => u.Courses)
            .ProjectTo<CourseProfileServiceModel>(this.mapper.ConfigurationProvider)
            .OrderByDescending(c => c.CourseStartDate)
            .ThenByDescending(c => c.CourseEndDate)
            .ProjectTo<CourseProfileMaxGradeServiceModel>(this.mapper.ConfigurationProvider)
            .ToListAsync();

        public IEnumerable<CertificatesByCourseServiceModel> GetCertificates(string id)
        {
            var certificatesQueryable = this.db
                .Certificates
                .Where(c => c.StudentId == id)
                .ProjectTo<CertificateDetailsListingServiceModel>(this.mapper.ConfigurationProvider)
                .AsEnumerable();

            var certificatesByCourse = certificatesQueryable
                .GroupBy(c => c.CourseId, c => c)
                .Select(g => new CertificatesByCourseServiceModel
                {
                    CourseId = g.Key,
                    CourseName = g.Select(c => c.CourseName).First(),
                    Certificates = g.OrderByDescending(c => c.IssueDate)
                })
                .OrderBy(c => c.CourseName)
                .ToList();

            return certificatesByCourse;
        }

        public async Task<IEnumerable<UserDiplomaListingServiceModel>> GetDiplomasAsync(string id)
            => await this.db
            .Diplomas
            .Where(d => d.StudentId == id)
            .OrderBy(d => d.Curriculum.Name)
            .ProjectTo<UserDiplomaListingServiceModel>(this.mapper.ConfigurationProvider)
            .ToListAsync();

        public IEnumerable<ExamsByCourseServiceModel> GetExams(string id)
        {
            var examsQueryable = this.db
                .ExamSubmissions
                .Where(e => e.StudentId == id)
                .ProjectTo<ExamSubmissionDetailsServiceModel>(this.mapper.ConfigurationProvider)
                .AsEnumerable();

            var examsByCourse = examsQueryable
                .GroupBy(r => r.CourseId, r => r)
                .Select(g => new ExamsByCourseServiceModel
                {
                    CourseId = g.Key,
                    CourseName = g.Select(r => r.CourseName).First(),
                    Exams = g.OrderByDescending(e => e.SubmissionDate)
                })
                .OrderBy(g => g.CourseName)
                .ToList();

            return examsByCourse;
        }

        public IEnumerable<ResourcesByCourseServiceModel> GetResources(string id)
        {
            var resources = this.GetUserById(id)
                .SelectMany(u => u.Courses)
                .SelectMany(sc => sc.Course.Resources)
                .ProjectTo<ResourceDetailsServiceModel>(this.mapper.ConfigurationProvider)
                .AsEnumerable();

            var resourcesByCourse = resources
                .GroupBy(r => r.CourseId, r => r)
                .Select(g => new ResourcesByCourseServiceModel
                {
                    CourseId = g.Key,
                    CourseName = g.Select(r => r.CourseName).First(),
                    Resources = g.OrderBy(r => r.FileName)
                })
                .OrderBy(g => g.CourseName)
                .ToList();

            return resourcesByCourse;
        }

        public async Task<bool> UpdateProfileAsync(string id, string name, DateTime birthdate)
        {
            var user = await this.db.Users.FindAsync(id);
            if (user == null
                || string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            user.Name = name.Trim();
            user.Birthdate = birthdate;

            var result = await this.db.SaveChangesAsync();
            var success = result > 0;

            return success;
        }

        private IQueryable<User> GetUserById(string id)
            => this.db.Users.Where(u => u.Id == id);
    }
}
