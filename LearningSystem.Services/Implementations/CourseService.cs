namespace LearningSystem.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using Microsoft.EntityFrameworkCore;

    public class CourseService : ICourseService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public CourseService(
            LearningSystemDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task AddExamSubmission(int id, string userId, byte[] examFileBytes)
        {
            if (!await this.IsUserEnrolledInCourseAsync(id, userId))
            {
                return;
            }

            var examSubmission = new ExamSubmission
            {
                CourseId = id,
                StudentId = userId,
                FileSubmission = examFileBytes,
                SubmissionDate = DateTime.UtcNow
            };

            await this.db.ExamSubmissions.AddAsync(examSubmission);
            await this.db.SaveChangesAsync();
        }

        public async Task<IEnumerable<CourseServiceModel>> AllActiveWithTrainersAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.AllWithTrainers(search, true, page, pageSize);

        public async Task<IEnumerable<CourseServiceModel>> AllArchivedWithTrainersAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.AllWithTrainers(search, false, page, pageSize);

        public async Task<bool> CanEnrollAsync(int id)
        {
            var course = await this.db.Courses.FindAsync(id);
            if (course == null)
            {
                return false;
            }

            return DateTime.UtcNow < course.StartDate; // has not started
        }

        public async Task CancellUserEnrollmentInCourseAsync(int courseId, string userId)
        {
            if (!await this.CanEnrollAsync(courseId)
                || !await this.db.Users.AnyAsync(u => u.Id == userId)
                || !await this.IsUserEnrolledInCourseAsync(courseId, userId))
            {
                return;
            }

            //var studentCourse = await this.db
            //    .Courses
            //    .Where(c => c.Id == courseId)
            //    .SelectMany(c => c.Students)
            //    .Where(sc => sc.StudentId == userId)
            //    .FirstOrDefaultAsync();

            // NB! observe primary key order in StudentCourse table
            var studentCourse = await this.db.FindAsync<StudentCourse>(userId, courseId);

            this.db.Remove(studentCourse);
            await this.db.SaveChangesAsync();
        }

        public async Task EnrollUserInCourseAsync(int courseId, string userId)
        {
            if (!await this.CanEnrollAsync(courseId)
                || !await this.db.Users.AnyAsync(u => u.Id == userId)
                || await this.IsUserEnrolledInCourseAsync(courseId, userId))
            {
                return;
            }

            await this.db.AddAsync(new StudentCourse { CourseId = courseId, StudentId = userId });
            await this.db.SaveChangesAsync();
        }

        public bool Exists(int id)
            => this.db.Courses.Any(c => c.Id == id);

        public async Task<CourseDetailsServiceModel> GetByIdAsync(int id)
            => await this.db
            .Courses
            .Where(c => c.Id == id)
            .Select(c => new CourseDetailsServiceModel
            {
                Course = this.mapper.Map<CourseWithDescriptionServiceModel>(c),
                Trainer = this.mapper.Map<UserServiceModel>(c.Trainer),
                Students = c.Students.Count()
            })
            .FirstOrDefaultAsync();

        public IQueryable<Course> GetQuerableBySearch(string search)
        {
            var coursesAsQuerable = this.db.Courses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                coursesAsQuerable = coursesAsQuerable
                    .Where(c => c.Name.ToLower().Contains(search.Trim().ToLower()))
                    .AsQueryable();
            }

            return coursesAsQuerable;
        }

        public IQueryable<Course> GetQuerableByStatus(IQueryable<Course> coursesAsQuerable, bool? isActive)
            => isActive == null // all
            ? coursesAsQuerable
            : (bool)isActive
                ? coursesAsQuerable
                    .Where(c => DateTime.UtcNow <= c.EndDate) // active
                    .AsQueryable()
                : coursesAsQuerable
                    .Where(c => c.EndDate < DateTime.UtcNow) // archive
                    .AsQueryable();

        public async Task<bool> IsUserEnrolledInCourseAsync(int courseId, string userId)
            => await this.db
            .Courses
            .AnyAsync(c => c.Id == courseId
                        && c.Students.Any(sc => sc.StudentId == userId));

        public async Task<int> TotalActiveAsync(string search = null)
            => await this.GetQuerableByStatus(this.GetQuerableBySearch(search), true)
            .CountAsync();

        public async Task<int> TotalArchivedAsync(string search = null)
            => await this.GetQuerableByStatus(this.GetQuerableBySearch(search), false)
            .CountAsync();

        private async Task<IEnumerable<CourseServiceModel>> AllWithTrainers(
            string search,
            bool isActive,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
        {
            var coursesAsQuerable = this.GetQuerableBySearch(search);

            var courses = await this.GetQuerableByStatus(coursesAsQuerable, isActive)
                .OrderByDescending(c => c.StartDate)
                .ThenByDescending(c => c.EndDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseListingServiceModel
                {
                    Course = this.mapper.Map<CourseServiceModel>(c),
                    Trainer = this.mapper.Map<UserBasicServiceModel>(c.Trainer)
                })
                .ToListAsync();

            for (var i = 0; i < courses.Count; i++)
            {
                var course = courses[i].Course;
                course.TrainerName = courses[i].Trainer.Name;
            }

            return courses.Select(c => c.Course).ToList();
        }
    }
}
