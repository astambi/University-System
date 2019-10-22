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
            => await this.db
            .ExamSubmissions
            .Where(e => e.CourseId == courseId)
            .Where(e => e.StudentId == userId)
            .OrderByDescending(e => e.SubmissionDate)
            .ProjectTo<ExamSubmissionServiceModel>(this.mapper.ConfigurationProvider)
            .ToListAsync();

        public async Task<bool> CanBeDownloadedByUserAsync(int id, string userId)
            => await this.db
            .ExamSubmissions
            .Where(e => e.Id == id)
            .AnyAsync(e => e.StudentId == userId // exam student
                || e.Course.TrainerId == userId); // course trainer

        public async Task<bool> CreateAsync(int courseId, string userId, string fileName, string fileUrl)
        {
            var studentCourseFound = await this.db
               .Courses
               .Where(c => c.Id == courseId)
               .Where(c => c.Students.Any(sc => sc.StudentId == userId))
               .AnyAsync();

            if (!studentCourseFound
                || string.IsNullOrWhiteSpace(fileName)
                || string.IsNullOrWhiteSpace(fileUrl))
            {
                return false;
            }

            var examSubmission = new ExamSubmission
            {
                CourseId = courseId,
                StudentId = userId,
                FileName = fileName,
                FileUrl = fileUrl,
                SubmissionDate = DateTime.UtcNow
            };

            await this.db.ExamSubmissions.AddAsync(examSubmission);
            var result = await this.db.SaveChangesAsync();
            var success = result > 0;

            return success;
        }

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
                .Where(c => c.EndDate < DateTime.UtcNow)
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

        public async Task<string> GetDownloadUrlAsync(int id)
            => await this.db
            .ExamSubmissions
            .Where(e => e.Id == id)
            .Select(e => e.FileUrl)
            .FirstOrDefaultAsync();

        private IQueryable<ExamSubmission> GetForStudentById(int id, string userId)
            => this.db
            .ExamSubmissions
            .Where(e => e.Id == id)
            .Where(e => e.StudentId == userId);
    }
}
