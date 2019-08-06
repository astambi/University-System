namespace LearningSystem.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Exams;
    using LearningSystem.Services.Models.Users;
    using Microsoft.EntityFrameworkCore;

    public class ExamService : IExamService
    {
        private readonly LearningSystemDbContext db;
        private readonly ICourseService courseService;
        private readonly IMapper mapper;

        public ExamService(
            LearningSystemDbContext db,
            ICourseService courseService,
            IMapper mapper)
        {
            this.db = db;
            this.courseService = courseService;
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
            if (!await this.courseService.IsUserEnrolledInCourseAsync(courseId, userId)
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

        public async Task<ExamDownloadServiceModel> DownloadAsync(int id)
            => await this.mapper
            .ProjectTo<ExamDownloadServiceModel>(
                this.db
                .ExamSubmissions
                .Where(e => e.Id == id))
            .FirstOrDefaultAsync();

        public async Task<bool> ExistsForStudentAsync(int id, string userId)
            => await this.db
            .ExamSubmissions
            .AnyAsync(e => e.Id == id
                && e.StudentId == userId);
    }
}
