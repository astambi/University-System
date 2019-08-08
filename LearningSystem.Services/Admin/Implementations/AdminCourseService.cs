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

        public async Task<int> CreateAsync(
            string name,
            string description,
            DateTime startDate, // local time
            DateTime endDate,   // local time
            decimal price,
            string trainerId)
        {
            var trainerExists = this.db.Users.Any(u => u.Id == trainerId);
            if (!trainerExists
                || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(description))
            {
                return int.MinValue;
            }

            var course = new Course
            {
                Name = name.Trim(),
                Description = description.Trim(),
                StartDate = startDate.ToStartDateUtc(),
                EndDate = endDate.ToEndDateUtc(),
                Price = price,
                TrainerId = trainerId
            };

            await this.db.Courses.AddAsync(course);
            var result = await this.db.SaveChangesAsync();

            return course.Id;
        }

        public async Task<AdminCourseServiceModel> GetByIdAsync(int id)
            => await this.mapper
            .ProjectTo<AdminCourseServiceModel>(this.db.Courses)
            .Where(c => c.Id == id)
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

        public async Task<bool> UpdateAsync(
            int id,
            string name,
            string description,
            DateTime startDate, // local time
            DateTime endDate, // local time
            decimal price,
            string trainerId)
        {
            var course = await this.db.Courses.FindAsync(id);
            if (course == null)
            {
                return false;
            }

            var trainerExists = this.db.Users.Any(u => u.Id == trainerId);
            if (!trainerExists)
            {
                return false;
            }

            course.Name = name;
            course.Description = description;
            course.StartDate = startDate.ToStartDateUtc();
            course.EndDate = endDate.ToEndDateUtc();
            course.TrainerId = trainerId;
            course.Price = price;

            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }
    }
}
