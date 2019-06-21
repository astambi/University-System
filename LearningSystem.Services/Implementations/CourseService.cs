namespace LearningSystem.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Services.Admin.Models;
    using LearningSystem.Services.Models;
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
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.db
            .Courses
            //.Where(c => DateTime.Compare(DateTime.UtcNow, c.EndDate) < 1) // active courses
            .OrderBy(c => c.StartDate)
            .ThenBy(c => c.EndDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CourseListingServiceModel
            {
                Course = this.mapper.Map<CourseServiceModel>(c),
                Trainer = this.mapper.Map<UserServiceModel>(c.Trainer),
            })
            .ToListAsync();

        public bool Exists(int id)
            => this.db.Courses.Any(c => c.Id == id);

        public async Task<int> TotalAsync()
            => await this.db.Courses
            //.Where(c => DateTime.Compare(DateTime.UtcNow, c.EndDate) < 1) // active courses
            .CountAsync();
    }
}
