namespace LearningSystem.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Common.Infrastructure.Extensions;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
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

        public async Task AddExamSubmissionAsync(int id, string userId, byte[] examFileBytes)
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

        public async Task<IEnumerable<CourseStudentExamSubmissionServiceModel>> ExamSubmisionsAsync(int courseId, string userId)
            => await this.mapper
            .ProjectTo<CourseStudentExamSubmissionServiceModel>(
                this.db
                .ExamSubmissions
                .Where(e => e.CourseId == courseId)
                .Where(e => e.StudentId == userId))
            .OrderByDescending(e => e.SubmissionDate)
            .ToListAsync();

        public bool Exists(int id)
            => this.db.Courses.Any(c => c.Id == id);

        public async Task<CourseDetailsServiceModel> GetByIdAsync(int id)
            => await this.mapper
            .ProjectTo<CourseDetailsServiceModel>(this.db.Courses)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        public IQueryable<Course> GetQueryableBySearch(string search)
        {
            var coursesAsQueryable = this.db.Courses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                coursesAsQueryable = coursesAsQueryable
                    .Where(c => c.Name.ToLower().Contains(search.Trim().ToLower()))
                    .AsQueryable();
            }

            return coursesAsQueryable;
        }

        public IQueryable<Course> GetQueryableByStatus(IQueryable<Course> coursesAsQueryable, bool? isActive)
            => isActive == null // all
            ? coursesAsQueryable
            : (bool)isActive
                ? coursesAsQueryable
                    .Where(c => DateTime.UtcNow <= c.EndDate) // active
                    .AsQueryable()
                : coursesAsQueryable
                    .Where(c => c.EndDate < DateTime.UtcNow) // archive
                    .AsQueryable();

        public bool IsGradeEligibleForCertificate(Grade? grade)
            => grade == Grade.A
            || grade == Grade.B
            || grade == Grade.C;

        public async Task<bool> IsUserEnrolledInCourseAsync(int courseId, string userId)
            => await this.db
            .Courses
            .AnyAsync(c =>
                c.Id == courseId
                && c.Students.Any(sc => sc.StudentId == userId));

        public async Task<int> TotalActiveAsync(string search = null)
            => await this.GetQueryableByStatus(this.GetQueryableBySearch(search), true)
            .CountAsync();

        public async Task<int> TotalArchivedAsync(string search = null)
            => await this.GetQueryableByStatus(this.GetQueryableBySearch(search), false)
            .CountAsync();

        private async Task<IEnumerable<CourseServiceModel>> AllWithTrainers(
            string search,
            bool isActive,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
        {
            // Negative page & page size
            page = Math.Max(1, page);

            if (pageSize < 1)
            {
                pageSize = ServicesConstants.PageSize;
            }

            var coursesSearchQueryable = this.GetQueryableBySearch(search);
            var coursesStatusQueryable = this.GetQueryableByStatus(coursesSearchQueryable, isActive);

            return await this.mapper
                .ProjectTo<CourseServiceModel>(coursesStatusQueryable)
                .OrderByDescending(c => c.StartDate)
                .ThenByDescending(c => c.EndDate)
                .GetPageItems(page, pageSize)
                .ToListAsync();
        }
    }
}
