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

        public async Task<IEnumerable<CourseListingServiceModel>> AllActiveWithTrainersAsync(
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.AllWithTrainers(true, page, pageSize);

        public async Task<IEnumerable<CourseListingServiceModel>> AllArchivedWithTrainersAsync(
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.AllWithTrainers(false, page, pageSize);

        public async Task<bool> CanEnrollAsync(int id)
        {
            var course = await this.db.Courses.FindAsync(id);
            if (course == null)
            {
                return false;
            }

            return course.StartDate > DateTime.UtcNow;
        }

        public async Task CancellUserEnrollmentInCourseAsync(int courseId, string userId)
        {
            if (!await this.CanEnrollAsync(courseId)
                || !await this.db.Users.AnyAsync(u => u.Id == userId)
                || !await this.UserIsEnrolledInCourseAsync(courseId, userId))
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
                || await this.UserIsEnrolledInCourseAsync(courseId, userId))
            {
                return;
            }

            await this.db.AddAsync(new StudentCourse { CourseId = courseId, StudentId = userId });
            await this.db.SaveChangesAsync();
        }

        public bool Exists(int id)
            => this.db.Courses.Any(c => c.Id == id);

        public async Task<CourseDetailsServiceModel> GetByIdAsync(int id)
            => await this.db.Courses
            .Where(c => c.Id == id)
            .Select(c => new CourseDetailsServiceModel
            {
                Course = this.mapper.Map<CourseWithDescriptionServiceModel>(c),
                Trainer = this.mapper.Map<UserServiceModel>(c.Trainer),
                Students = c.Students.Count()
            })
            .FirstOrDefaultAsync();

        public async Task<bool> UserIsEnrolledInCourseAsync(int courseId, string userId)
            => await this.db
            .Courses
            .AnyAsync(c => c.Id == courseId
                        && c.Students.Any(sc => sc.StudentId == userId));

        public async Task<int> TotalActiveAsync()
            => await this.TotalAsync(true);

        public async Task<int> TotalArchivedAsync()
            => await this.TotalAsync(false);

        private async Task<IEnumerable<CourseListingServiceModel>> AllWithTrainers(
            bool isActive,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.GetQuerableCoursesSelection(isActive)
            .OrderBy(c => c.StartDate)
            .ThenBy(c => c.EndDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CourseListingServiceModel
            {
                Course = this.mapper.Map<CourseServiceModel>(c),
                Trainer = this.mapper.Map<UserBasicServiceModel>(c.Trainer),
            })
            .ToListAsync();

        private async Task<int> TotalAsync(bool isActive)
            => await this.GetQuerableCoursesSelection(isActive).CountAsync();

        private IQueryable<Course> GetQuerableCoursesSelection(bool isActive)
        {
            var coursesAsQuerable = this.db.Courses.AsQueryable();

            coursesAsQuerable = isActive
                ? coursesAsQuerable
                    .Where(c => DateTime.Compare(DateTime.UtcNow, c.EndDate.AddDays(1)) < 1) // EndDate time in db = 00:00:00
                    .AsQueryable()
                : coursesAsQuerable
                    .Where(c => DateTime.Compare(DateTime.UtcNow, c.EndDate.AddDays(1)) == 1)
                    .AsQueryable();

            return coursesAsQuerable;
        }
    }
}
