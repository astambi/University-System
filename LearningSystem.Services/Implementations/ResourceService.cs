namespace LearningSystem.Services.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Resources;
    using Microsoft.EntityFrameworkCore;

    public class ResourceService : IResourceService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public ResourceService(
            LearningSystemDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<CourseResourceServiceModel>> AllByCourseAsync(int courseId)
            => await this.mapper
            .ProjectTo<CourseResourceServiceModel>(
                this.db
                .Resources
                .Where(r => r.CourseId == courseId))
            .OrderBy(r => r.FileName)
            .ToListAsync();

        public async Task<bool> CanBeDownloadedByUserAsync(int id, string userId)
            => await this.db
            .Resources
            .AnyAsync(r => r.Id == id
                && (r.Course.TrainerId == userId // trainers or enrolled students only
                || r.Course.Students.Any(sc => sc.StudentId == userId)));

        public async Task<bool> CreateAsync(int courseId, string fileName, string contentType, byte[] fileBytes)
        {
            var courseExists = this.db.Courses.Any(c => c.Id == courseId);
            if (!courseExists
                || string.IsNullOrWhiteSpace(fileName)
                || string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }

            var resource = new Resource
            {
                CourseId = courseId,
                FileName = fileName.Trim(),
                ContentType = contentType,
                FileBytes = fileBytes,
            };

            await this.db.Resources.AddAsync(resource);
            var result = await this.db.SaveChangesAsync();

            var success = result > 0;
            return success;
        }

        public async Task<ResourceDownloadServiceModel> DownloadAsync(int id)
            => await this.mapper
            .ProjectTo<ResourceDownloadServiceModel>(
                this.db
                .Resources
                .Where(r => r.Id == id))
            .FirstOrDefaultAsync();

        public bool Exists(int id)
            => this.db.Resources.Any(r => r.Id == id);

        public async Task<bool> RemoveAsync(int id)
        {
            var resource = await this.db.Resources.FindAsync(id);
            if (resource == null)
            {
                return false;
            }

            this.db.Resources.Remove(resource);
            var result = await this.db.SaveChangesAsync();

            var success = result > 0;
            return success;
        }
    }
}
