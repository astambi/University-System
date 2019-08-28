namespace University.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using University.Common.Infrastructure.Extensions;
    using University.Data;
    using University.Data.Models;
    using University.Services.Models.Exams;

    public class ExamService : IExamService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public ExamService(
            UniversityDbContext db,
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

        public async Task<bool> CreateAsync(int courseId, string userId, byte[] examFileBytes)
        {
            var studentCourseFound = await this.db
                .Courses
                .Where(c => c.Id == courseId)
                .Where(c => c.Students.Any(sc => sc.StudentId == userId))
                .AnyAsync();

            if (!studentCourseFound
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
            .ProjectTo<ExamDownloadServiceModel>(this.GetForStudentById(id, userId))
            .FirstOrDefaultAsync();

        public async Task<ExamDownloadServiceModel> DownloadForTrainerAsync(string trainerId, int courseId, string studentId)
            => await this.mapper
            .ProjectTo<ExamDownloadServiceModel>(
                this.db
                .ExamSubmissions
                .Where(e => e.CourseId == courseId)
                .Where(e => e.StudentId == studentId)
                .Where(e => e.Course.TrainerId == trainerId)
                .Where(e => e.Course.EndDate.HasEnded()))
            .OrderByDescending(e => e.SubmissionDate) // latest submission
            .FirstOrDefaultAsync();

        public async Task<bool> ExistsForStudentAsync(int id, string userId)
            => await this.GetForStudentById(id, userId)
            .AnyAsync();

        public async Task<bool> EvaluateAsync(string trainerId, int courseId, string studentId, decimal gradeBg)
        {
            var isCourseTrainer = await this.db
                .Courses
                .Where(c => c.Id == courseId)
                .Where(c => c.TrainerId == trainerId)
                .AnyAsync();

            var courseHasEnded = await this.db
                .Courses
                .Where(c => c.Id == courseId)
                .Where(c => c.EndDate.HasEnded())
                .AnyAsync();

            var studentCourse = await this.db.FindAsync<StudentCourse>(studentId, courseId);

            var examsFound = await this.db
                .ExamSubmissions
                .Where(e => e.CourseId == courseId)
                .Where(e => e.StudentId == studentId)
                .AnyAsync();

            if (!(isCourseTrainer && courseHasEnded)
                || studentCourse == null
                || !examsFound
                || gradeBg < DataConstants.GradeBgMinValue
                || gradeBg > DataConstants.GradeBgMaxValue)
            {
                return false;
            }

            if (studentCourse.GradeBg == gradeBg)
            {
                return true;
            }

            studentCourse.GradeBg = gradeBg;
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        private IQueryable<ExamSubmission> GetForStudentById(int id, string userId)
            => this.db
            .ExamSubmissions
            .Where(e => e.Id == id)
            .Where(e => e.StudentId == userId);
    }
}
