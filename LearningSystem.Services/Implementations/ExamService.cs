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
    using LearningSystem.Services.Models.Exams;
    using Microsoft.EntityFrameworkCore;

    public class ExamService : IExamService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public ExamService(
            LearningSystemDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<ExamSubmissionServiceModel>> AllByStudentCourseAsync(int courseId, string userId)
            => await this.mapper
            .ProjectTo<ExamSubmissionServiceModel>(
                this.db
                .ExamSubmissions
                .Where(e => e.CourseId == courseId)
                .Where(e => e.StudentId == userId))
            .OrderByDescending(e => e.SubmissionDate)
            .ToListAsync();

        public async Task<bool> AssessAsync(string trainerId, int courseId, string studentId, Grade grade)
        {
            var isTrainer = await this.db
                .Courses
                .Where(c => c.Id == courseId)
                .Where(c => c.TrainerId == trainerId)
                .AnyAsync();

            var courseHasEnded = await this.db
                .Courses
                .Where(c => c.Id == courseId)
                .Where(c => c.EndDate.HasEnded())
                .AnyAsync();

            if (!(isTrainer && courseHasEnded))
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

        public async Task<bool> CreateAsync(int courseId, string userId, byte[] examFileBytes)
        {
            if (!this.db.Courses.Any(c => c.Id == courseId && c.Students.Any(sc => sc.StudentId == userId))
                || examFileBytes == null
                || examFileBytes.Length == 0
                || examFileBytes.Length > DataConstants.FileMaxLengthInBytes)
            {
                return false;
            }

            var examSubmission = new ExamSubmission
            {
                CourseId = courseId,
                StudentId = userId,
                FileSubmission = examFileBytes,
                SubmissionDate = DateTime.UtcNow
            };

            await this.db.ExamSubmissions.AddAsync(examSubmission);
            var result = await this.db.SaveChangesAsync();
            var success = result > 0;

            return success;
        }

        public async Task<ExamDownloadServiceModel> DownloadForStudentAsync(int id, string userId)
            => await this.mapper
            .ProjectTo<ExamDownloadServiceModel>(
                this.db
                .ExamSubmissions
                .Where(e => e.Id == id)
                .Where(e => e.StudentId == userId))
            .FirstOrDefaultAsync();

        public async Task<ExamDownloadServiceModel> DownloadForTrainerAsync(string trainerId, int courseId, string studentId)
            => await this.mapper
            .ProjectTo<ExamDownloadServiceModel>(
                this.db
                .ExamSubmissions
                .Where(e => e.CourseId == courseId)
                .Where(e => e.StudentId == studentId)
                .Where(e => e.Course.TrainerId == trainerId) // by course trainer only
                .Where(e => e.Course.EndDate.HasEnded())) // after course end only
            .OrderByDescending(e => e.SubmissionDate) // latest submission
            .FirstOrDefaultAsync();

        public async Task<bool> ExistsForStudentAsync(int id, string userId)
            => await this.db
            .ExamSubmissions
            .AnyAsync(e =>
                e.Id == id
                && e.StudentId == userId);
    }
}
