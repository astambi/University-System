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

    public class TrainerService : ITrainerService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;
        private readonly ICourseService courseService;

        public TrainerService(
            LearningSystemDbContext db,
            IMapper mapper,
            ICourseService courseService)
        {
            this.db = db;
            this.mapper = mapper;
            this.courseService = courseService;
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
                .Where(c => c.CourseId == courseId && c.StudentId == studentId)
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

        public async Task<bool> AssessStudentCoursePerformanceAsync(string trainerId, int courseId, string studentId, Grade grade)
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

            studentCourse.Grade = grade;
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<IEnumerable<CourseServiceModel>> CoursesAsync(
            string trainerId,
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.courseService
            .GetQuerableBySearch(search)
            .Where(c => c.TrainerId == trainerId)
            .OrderByDescending(c => c.StartDate)
            .ThenByDescending(c => c.EndDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => this.mapper.Map<CourseServiceModel>(c))
            .ToListAsync();

        public async Task<CourseServiceModel> CourseByIdAsync(string trainerId, int courseId)
            => await this.db
            .Courses
            .Where(c => c.Id == courseId)
            .Where(c => c.TrainerId == trainerId)
            .Select(c => this.mapper.Map<CourseServiceModel>(c))
            .FirstOrDefaultAsync();

        public async Task<CourseWithResourcesServiceModel> CourseWithResourcesByIdAsync(string trainerId, int courseId)
            => await this.db
            .Courses
            .Where(c => c.Id == courseId)
            .Where(c => c.TrainerId == trainerId)
            .Select(c => new CourseWithResourcesServiceModel
            {
                Course = this.mapper.Map<CourseServiceModel>(c),
                Resources = c.Resources
                    .Select(r => this.mapper.Map<CourseResourceServiceModel>(r))
                    .OrderBy(r => r.FileName)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        public async Task<bool> CourseHasEndedAsync(int id)
            => await this.db
            .Courses
            .AnyAsync(c => c.Id == id && c.EndDate < DateTime.UtcNow);

        public async Task<ExamDownloadServiceModel> DownloadExam(string trainerId, int courseId, string studentId)
            => await this.IsTrainerForCourseAsync(trainerId, courseId)
            && await this.CourseHasEndedAsync(courseId)
            ? await this.db
                .ExamSubmissions
                .Where(e => e.CourseId == courseId)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.SubmissionDate)
                .Select(e => new ExamDownloadServiceModel
                {
                    FileSubmission = e.FileSubmission,
                    SubmissionDate = e.SubmissionDate,
                    Student = e.Student.UserName,
                    Course = e.Course.Name
                })
                .FirstOrDefaultAsync()
            : null;

        public async Task<bool> IsTrainerForCourseAsync(string userId, int courseId)
            => await this.db
            .Courses
            .AnyAsync(c => c.Id == courseId && c.TrainerId == userId);

        public async Task<IEnumerable<StudentInCourseServiceModel>> StudentsInCourseAsync(int courseId)
            => await this.db
            .Courses
            .Where(c => c.Id == courseId)
            .SelectMany(c => c.Students)
            .Select(sc => new StudentInCourseServiceModel
            {
                Student = this.mapper.Map<UserServiceModel>(sc.Student),
                Grade = sc.Grade,
                HasExamSubmission = sc
                    .Student
                    .ExamSubmissions
                    .Any(e => e.CourseId == sc.CourseId)
            })
            .ToListAsync();

        public async Task<int> TotalCoursesAsync(string trainerId, string search = null)
            => await this.courseService
            .GetQuerableBySearch(search)
            .Where(c => c.TrainerId == trainerId)
            .CountAsync();
    }
}
