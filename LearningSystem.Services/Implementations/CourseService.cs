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

        public async Task<IEnumerable<CourseListingServiceModel>> AllWithTrainers(
            bool activeOnly = true,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.GetQuerableCoursesSelection(activeOnly)
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

        public bool Exists(int id)
            => this.db.Courses.Any(c => c.Id == id);

        public async Task<CourseDetailsServiceModel> GetById(int id)
            => await this.db.Courses
            .Where(c => c.Id == id)
            .Select(c => new CourseDetailsServiceModel
            {
                Course = this.mapper.Map<CourseWithDescriptionServiceModel>(c),
                Trainer = this.mapper.Map<UserServiceModel>(c.Trainer),
                Students = c.Students.Count()
            })
            .FirstOrDefaultAsync();

        public async Task<int> TotalAsync(bool activeOnly = true)
            => await this.GetQuerableCoursesSelection(activeOnly).CountAsync();

        private IQueryable<Course> GetQuerableCoursesSelection(bool activeOnly)
        {
            var coursesAsQuerable = this.db.Courses.AsQueryable();

            if (activeOnly)
            {
                coursesAsQuerable = coursesAsQuerable
                    .Where(c => DateTime.Compare(DateTime.UtcNow, c.EndDate) < 1)
                    .AsQueryable();
            }

            return coursesAsQuerable;
        }
    }
}
