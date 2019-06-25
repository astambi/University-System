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

        public async Task AssessStudentCoursePerformance(string trainerId, int courseId, string studentId, Grade grade)
        {
            if (!await this.IsTrainerForCourseAsync(trainerId, courseId)
                || !await this.CourseHasEnded(courseId))
            {
                return;
            }

            var studentCourse = await this.db.FindAsync<StudentCourse>(studentId, courseId);
            if (studentCourse == null)
            {
                return;
            }

            studentCourse.Grade = grade;
            await this.db.SaveChangesAsync();
        }

        public async Task<CourseServiceModel> CourseAsync(string id, int courseId)
            => await this.db
            .Courses
            .Where(c => c.Id == courseId)
            .Where(c => c.TrainerId == id)
            .Select(c => this.mapper.Map<CourseServiceModel>(c))
            .FirstOrDefaultAsync();

        public async Task<IEnumerable<CourseServiceModel>> CoursesAsync(
            string id,
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.courseService.GetQuerableBySearch(search)
            .Where(c => c.TrainerId == id)
            .OrderByDescending(c => c.StartDate)
            .ThenByDescending(c => c.EndDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => this.mapper.Map<CourseServiceModel>(c))
            .ToListAsync();

        public async Task<bool> CourseHasEnded(int courseId)
            => await this.db.Courses
            .AnyAsync(c => c.Id == courseId && c.EndDate < DateTime.UtcNow);

        public async Task<bool> IsTrainerForCourseAsync(string userId, int courseId)
            => await this.db.Courses.AnyAsync(c => c.Id == courseId && c.TrainerId == userId);

        public async Task<IEnumerable<StudentInCourseServiceModel>> StudentsInCourseAsync(int courseId)
            => await this.db
            .Courses
            .Where(c => c.Id == courseId)
            .SelectMany(c => c.Students)
            .Select(sc => new StudentInCourseServiceModel
            {
                Student = this.mapper.Map<UserServiceModel>(sc.Student),
                Grade = sc.Grade
            })
            .ToListAsync();

        public async Task<int> TotalCoursesAsync(string id, string search = null)
            => await this.courseService.GetQuerableBySearch(search)
            .Where(c => c.TrainerId == id)
            .CountAsync();
    }
}
