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
    using LearningSystem.Services.Models.Users;
    using Microsoft.EntityFrameworkCore;

    public class TrainerService : ITrainerService
    {
        private readonly LearningSystemDbContext db;
        private readonly ICourseService courseService;
        private readonly IMapper mapper;

        public TrainerService(
            LearningSystemDbContext db,
            ICourseService courseService,
            IMapper mapper)
        {
            this.db = db;
            this.courseService = courseService;
            this.mapper = mapper;
        }

        public async Task<bool> AddCertificateAsync(string trainerId, int courseId, string studentId, Grade grade)
        {
            if (!await this.IsTrainerForCourseAsync(trainerId, courseId)
                || !await this.courseService.IsUserEnrolledInCourseAsync(courseId, studentId)
                || !this.courseService.IsGradeEligibleForCertificate(grade))
            {
                return false;
            }

            var prevBestCertificate = await this.db
                .Certificates
                .Where(c => c.CourseId == courseId)
                .Where(c => c.StudentId == studentId)
                .OrderBy(c => c.Grade)
                .FirstOrDefaultAsync();

            var canUpgradeCertificate =
                prevBestCertificate == null // no prev certificate
                || grade < prevBestCertificate.Grade; // Enum Grade value smaller is better (A = 0, B = 1, etc.)

            if (!canUpgradeCertificate)
            {
                return false;
            }

            var certificate = new Certificate
            {
                Id = Guid.NewGuid().ToString().Replace("-", string.Empty),
                StudentId = studentId,
                CourseId = courseId,
                Grade = grade,
                IssueDate = DateTime.UtcNow
            };

            await this.db.Certificates.AddAsync(certificate);
            await this.db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssessExamAsync(string trainerId, int courseId, string studentId, Grade grade)
        {
            if (!await this.IsTrainerForCourseAsync(trainerId, courseId)
                || !await this.CourseHasEndedAsync(courseId))
            {
                return false;
            }

            var studentCourse = await this.db.FindAsync<StudentCourse>(studentId, courseId);
            if (studentCourse == null)
            {
                return false;
            }

            if (studentCourse.Grade == grade)
            {
                return true;
            }

            studentCourse.Grade = grade;
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<CourseServiceModel> CourseByIdAsync(string trainerId, int courseId)
            => await this.mapper
            .ProjectTo<CourseServiceModel>(this.GetTrainerCourseQueryable(trainerId, courseId))
            .FirstOrDefaultAsync();

        public async Task<CourseWithResourcesServiceModel> CourseWithResourcesByIdAsync(string trainerId, int courseId)
            => await this.mapper
            .ProjectTo<CourseWithResourcesServiceModel>(this.GetTrainerCourseQueryable(trainerId, courseId))
            .FirstOrDefaultAsync();

        public async Task<bool> CourseHasEndedAsync(int id)
            => await this.db
            .Courses
            .AnyAsync(c =>
                c.Id == id
                && c.EndDate < DateTime.UtcNow);

        public async Task<IEnumerable<CourseServiceModel>> CoursesAsync(
            string trainerId,
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.mapper
            .ProjectTo<CourseServiceModel>(this.courseService.GetQueryableBySearch(search))
            .Where(c => c.TrainerId == trainerId)
            .OrderByDescending(c => c.StartDate)
            .ThenByDescending(c => c.EndDate)
            .GetPageItems(page, pageSize)
            .ToListAsync();

        public async Task<ExamDownloadServiceModel> DownloadExam(string trainerId, int courseId, string studentId)
            => await this.IsTrainerForCourseAsync(trainerId, courseId)
            && await this.CourseHasEndedAsync(courseId)
            ? await this.mapper
                .ProjectTo<ExamDownloadServiceModel>(
                    this.db
                    .ExamSubmissions
                    .Where(e => e.CourseId == courseId)
                    .Where(e => e.StudentId == studentId))
                .OrderByDescending(e => e.SubmissionDate)
                .FirstOrDefaultAsync()
            : null;

        public async Task<bool> IsTrainerForCourseAsync(string userId, int courseId)
            => await this.db
            .Courses
            .AnyAsync(c => c.Id == courseId && c.TrainerId == userId);

        public async Task<IEnumerable<StudentInCourseServiceModel>> StudentsInCourseAsync(int courseId)
            => await this.mapper
            .ProjectTo<StudentInCourseServiceModel>(
                this.db
                .Courses
                .Where(c => c.Id == courseId)
                .SelectMany(c => c.Students))
            .ToListAsync();

        public async Task<int> TotalCoursesAsync(string trainerId, string search = null)
            => await this.courseService
            .GetQueryableBySearch(search)
            .Where(c => c.TrainerId == trainerId)
            .CountAsync();

        private IQueryable<Course> GetTrainerCourseQueryable(string trainerId, int courseId)
            => this.db
            .Courses
            .Where(c => c.Id == courseId)
            .Where(c => c.TrainerId == trainerId);
    }
}
