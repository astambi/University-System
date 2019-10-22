namespace University.Services.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Data;
    using University.Data.Models;
    using University.Services.Models.Resources;

    public class ResourceService : IResourceService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public ResourceService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<ResourceServiceModel>> AllByCourseAsync(int courseId)
            => await this.db
            .Resources
            .Where(r => r.CourseId == courseId)
            .OrderBy(r => r.FileName)
            .ProjectTo<ResourceServiceModel>(this.mapper.ConfigurationProvider)
            .ToListAsync();

        public async Task<bool> CanBeDownloadedByUserAsync(int id, string userId)
            => await this.db
            .Resources
            .Where(r => r.Id == id)
            .AnyAsync(r => r.Course.TrainerId == userId // course trainer
                || r.Course.Students.Any(sc => sc.StudentId == userId)); // enrolled student

        public async Task<bool> CreateAsync(int courseId, string fileName, string fileUrl)
        {
            var courseExists = this.db.Courses.Any(c => c.Id == courseId);
            if (!courseExists
                || string.IsNullOrWhiteSpace(fileName)
                || string.IsNullOrWhiteSpace(fileUrl))
            {
                return false;
            }

            var resource = new Resource
            {
                FileName = fileName.Trim(),
                CourseId = courseId,
                FileUrl = fileUrl
            };

            await this.db.Resources.AddAsync(resource);
            var result = await this.db.SaveChangesAsync();

            var success = result > 0;
            return success;
        }

        public bool Exists(int id)
            => this.db.Resources.Any(r => r.Id == id);

        public async Task<string> GetDownloadUrlAsync(int id)
            => await this.db
            .Resources
            .Where(r => r.Id == id)
            .Select(r => r.FileUrl)
            .FirstOrDefaultAsync();

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
