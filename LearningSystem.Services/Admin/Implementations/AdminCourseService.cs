namespace LearningSystem.Services.Admin.Implementations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Common.Infrastructure.Extensions;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Admin.Models;
    using Microsoft.EntityFrameworkCore;

    public class AdminCourseService : IAdminCourseService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public AdminCourseService(LearningSystemDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task CreateAsync(
            string name,
            string description,
            DateTime startDate, // local time
            DateTime endDate, // local time
            string trainerId)
        {
            var trainerExists = this.db.Users.Any(u => u.Id == trainerId);
            if (!trainerExists)
            {
                return;
            }

            var course = new Course
            {
                Name = name,
                Description = description,
                StartDate = startDate.ToStartDateUtc(),
                EndDate = endDate.ToEndDateUtc(),
                TrainerId = trainerId
            };

            await this.db.Courses.AddAsync(course);
            await this.db.SaveChangesAsync();
        }

        public async Task<AdminCourseServiceModel> GetByIdAsync(int id)
            => await this.db.Courses
            .Where(c => c.Id == id)
            .Select(c => this.mapper.Map<AdminCourseServiceModel>(c))
            .FirstOrDefaultAsync();

        public async Task RemoveAsync(int id)
        {
            var course = await this.db.Courses.FindAsync(id);
            if (course == null)
            {
                return;
            }

            this.db.Courses.Remove(course);
            await this.db.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            int id,
            string name,
            string description,
            DateTime startDate, // local time
            DateTime endDate, // local time
            string trainerId)
        {
            var course = await this.db.Courses.FindAsync(id);
            if (course == null)
            {
                return;
            }

            var trainerExists = this.db.Users.Any(u => u.Id == trainerId);
            if (!trainerExists)
            {
                return;
            }

            course.Name = name;
            course.Description = description;
            course.StartDate = startDate.ToStartDateUtc();
            course.EndDate = endDate.ToEndDateUtc();
            course.TrainerId = trainerId;

            await this.db.SaveChangesAsync();
        }
    }
}
