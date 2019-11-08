namespace University.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Common.Infrastructure.Extensions;
    using University.Data;
    using University.Data.Models;
    using University.Services.Models.Certificates;
    using University.Services.Models.Courses;
    using University.Services.Models.Users;

    public class TrainerService : ITrainerService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public TrainerService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<CourseServiceModel> CourseByIdAsync(string trainerId, int courseId)
            => await this.GetTrainerCourse(trainerId, courseId)
            .ProjectTo<CourseServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public async Task<CourseWithResourcesServiceModel> CourseWithResourcesByIdAsync(string trainerId, int courseId)
            => await this.GetTrainerCourse(trainerId, courseId)
            .ProjectTo<CourseWithResourcesServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public async Task<bool> CourseHasEndedAsync(int id)
            => await this.db
            .Courses
            .Where(c => c.Id == id)
            .Where(c => c.EndDate < DateTime.UtcNow)
            .AnyAsync();

        public async Task<IEnumerable<CourseServiceModel>> CoursesAsync(
            string trainerId,
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.GetTrainerCoursesBySearch(trainerId, search)
            .OrderByDescending(c => c.StartDate)
            .ThenByDescending(c => c.EndDate)
            .ProjectTo<CourseServiceModel>(this.mapper.ConfigurationProvider)
            .GetPageItems(page, pageSize)
            .ToListAsync();

        public async Task<IEnumerable<CourseServiceModel>> CoursesToEvaluateAsync(string trainerId)
            => await this.GetTrainerCourses(trainerId)
            .Where(c => c.EndDate < DateTime.UtcNow && DateTime.UtcNow <= c.EndDate.AddDays(ServicesConstants.EvaluationPeriodInDays))
            .OrderBy(c => c.Name)
            .ProjectTo<CourseServiceModel>(this.mapper.ConfigurationProvider)
            .ToListAsync();

        public async Task<UserServiceModel> GetProfileAsync(string trainerId)
            => await this.db.Users
            .Where(u => u.Id == trainerId)
            .ProjectTo<UserServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public async Task<bool> IsTrainerForCourseAsync(string userId, int courseId)
            => await this.GetTrainerCourse(userId, courseId)
            .AnyAsync();

        public async Task<IEnumerable<StudentInCourseServiceModel>> StudentsInCourseAsync(int courseId)
        {
            var students = await this.db
               .Courses
               .Where(c => c.Id == courseId)
               .Where(c => c.Students.Any())
               .SelectMany(c => c.Students)
               .OrderBy(s => s.Student.UserName)
               .ProjectTo<StudentInCourseServiceModel>(this.mapper.ConfigurationProvider)
               .ToListAsync();

            /// Getting certificates with a second query as EF Core 3.0 cannot evaluate previous AutoMapper query 
            var studentIds = students.Select(s => s.StudentId);

            var certificatesByStudent = this.db
                .Certificates
                .Where(c => c.CourseId == courseId)
                .Where(c => studentIds.Contains(c.StudentId))
                .ProjectTo<CertificateListingServiceModel>(this.mapper.ConfigurationProvider)
                .ToList()
                .GroupBy(c => c.StudentId, c => c)
                .ToDictionary(
                    g => g.Key, // studentId
                    g => g.OrderByDescending(c => c.GradeBg)); // certificates

            for (var i = 0; i < students.Count; i++)
            {
                var student = students[i];
                if (certificatesByStudent.ContainsKey(student.StudentId))
                {
                    student.Certificates = certificatesByStudent[student.StudentId];
                }
            }

            return students;
        }

        public async Task<int> TotalCoursesAsync(string trainerId, string search = null)
            => await this.GetTrainerCoursesBySearch(trainerId, search)
            .CountAsync();

        private IQueryable<Course> GetTrainerCourse(string trainerId, int courseId)
            => this.db
            .Courses
            .Where(c => c.Id == courseId)
            .Where(c => c.TrainerId == trainerId);

        private IQueryable<Course> GetTrainerCourses(string trainerId)
            => this.db
            .Courses
            .Where(c => c.TrainerId == trainerId);

        private IQueryable<Course> GetTrainerCoursesBySearch(string trainerId, string search)
            => string.IsNullOrWhiteSpace(search)
            ? this.GetTrainerCourses(trainerId)
            : this.GetTrainerCourses(trainerId)
                .Where(c => c.Name.ToLower().Contains(search.Trim().ToLower()));
    }
}
