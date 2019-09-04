namespace University.Services.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using University.Common.Infrastructure.Extensions;
    using University.Data;
    using University.Data.Models;
    using University.Services.Models.Courses;
    using University.Services.Models.Users;

    public class TrainerService : ITrainerService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public TrainerService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<CourseServiceModel> CourseByIdAsync(string trainerId, int courseId)
            => await this.mapper
            .ProjectTo<CourseServiceModel>(this.GetTrainerCourse(trainerId, courseId))
            .FirstOrDefaultAsync();

        public async Task<CourseWithResourcesServiceModel> CourseWithResourcesByIdAsync(string trainerId, int courseId)
            => await this.mapper
            .ProjectTo<CourseWithResourcesServiceModel>(this.GetTrainerCourse(trainerId, courseId))
            .FirstOrDefaultAsync();

        public async Task<bool> CourseHasEndedAsync(int id)
            => await this.db
            .Courses
            .Where(c => c.Id == id)
            .Where(c => c.EndDate.HasEnded())
            .AnyAsync();

        public async Task<IEnumerable<CourseServiceModel>> CoursesAsync(
            string trainerId,
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.mapper
            .ProjectTo<CourseServiceModel>(
                this.GetTrainerCoursesBySearch(trainerId, search)
                .OrderByDescending(c => c.StartDate)
                .ThenByDescending(c => c.EndDate))
            .GetPageItems(page, pageSize)
            .ToListAsync();

        public async Task<IEnumerable<CourseServiceModel>> CoursesToEvaluateAsync(string trainerId)
            => await this.mapper
            .ProjectTo<CourseServiceModel>(this.GetTrainerCourses(trainerId))
            .Where(c => c.CanBeEvaluated)
            .OrderBy(c => c.Name)
            .ToListAsync();

        public async Task<UserServiceModel> GetProfileAsync(string trainerId)
            => await this.mapper
            .ProjectTo<UserServiceModel>(this.db.Users.Where(u => u.Id == trainerId))
            .FirstOrDefaultAsync();

        public async Task<bool> IsTrainerForCourseAsync(string userId, int courseId)
            => await this.GetTrainerCourse(userId, courseId)
            .AnyAsync();

        public async Task<IEnumerable<StudentInCourseServiceModel>> StudentsInCourseAsync(int courseId)
            => await this.mapper
            .ProjectTo<StudentInCourseServiceModel>(
                this.db
                .Courses
                .Where(c => c.Id == courseId)
                .SelectMany(c => c.Students)
                .OrderBy(sc => sc.Student.UserName))
            .ToListAsync();

        public async Task<int> TotalCoursesAsync(string trainerId, string search = null)
            => await this.GetTrainerCoursesBySearch(trainerId, search)
            .CountAsync();

        private IQueryable<Course> GetTrainerCourse(string trainerId, int courseId)
            => this.db
            .Courses
            .Where(c => c.Id == courseId)
            .Where(c => c.TrainerId == trainerId);

        private IQueryable<Course> GetTrainerCourses(string trainerId)
            => this.db
            .Courses
            .Where(c => c.TrainerId == trainerId);

        private IQueryable<Course> GetTrainerCoursesBySearch(string trainerId, string search)
            => string.IsNullOrWhiteSpace(search)
            ? this.GetTrainerCourses(trainerId)
            : this.GetTrainerCourses(trainerId)
                .Where(c => c.Name.ToLower().Contains(search.Trim().ToLower()));
    }
}
