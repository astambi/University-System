namespace University.Services.Admin.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Data;
    using University.Data.Models;
    using University.Services.Admin.Models.Curriculums;
    using University.Services.Admin.Models.Users;

    public class AdminCurriculumService : IAdminCurriculumService
    {
        private const int ResultInvalidId = int.MinValue;

        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public AdminCurriculumService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<bool> AddCourseAsync(int curriculumId, int courseId)
        {
            var curriculumExists = await this.ExistsAsync(curriculumId);
            var courseExists = await this.db.Courses.AnyAsync(c => c.Id == courseId);
            var curriculumCourseExists = await this.ExistsCurriculumCourseAsync(curriculumId, courseId);

            if (!curriculumExists
                || !courseExists
                || curriculumCourseExists)
            {
                return false;
            }

            await this.db.AddAsync(new CurriculumCourse { CurriculumId = curriculumId, CourseId = courseId });
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<IEnumerable<AdminCurriculumServiceModel>> AllAsync()
            => await this.db
            .Curriculums
            .OrderBy(c => c.Name)
            .ProjectTo<AdminCurriculumServiceModel>(this.mapper.ConfigurationProvider)
            .ToListAsync();

        public async Task<int> CreateAsync(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(description))
            {
                return ResultInvalidId;
            }

            var curriculum = new Curriculum
            {
                Name = name.Trim(),
                Description = description.Trim(),
            };

            await this.db.Curriculums.AddAsync(curriculum);
            var result = await this.db.SaveChangesAsync();

            return curriculum.Id;
        }

        public async Task<bool> ExistsAsync(int id)
            => await this.db.Curriculums.AnyAsync(c => c.Id == id);

        public async Task<bool> ExistsCurriculumCourseAsync(int curriculumId, int courseId)
            => await this.db
            .FindAsync<CurriculumCourse>(curriculumId, courseId) != null;

        public async Task<AdminCurriculumBasicServiceModel> GetByIdAsync(int id)
            => await this.db
            .Curriculums
            .Where(c => c.Id == id)
            .ProjectTo<AdminCurriculumBasicServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public async Task<IEnumerable<AdminDiplomaGraduateServiceModel>> GetDiplomaGraduatesAsync(int id)
            => await this.db
            .Diplomas
            .Where(d => d.CurriculumId == id)
            .OrderBy(u => u.Student.Name)
            .ProjectTo<AdminDiplomaGraduateServiceModel>(this.mapper.ConfigurationProvider)
            .ToListAsync();

        public async Task<IEnumerable<AdminUserListingServiceModel>> GetEligibleCandidatesWithoutDiplomasAsync(int id)
        {
            var curriculumCourses = this.db
                .Curriculums
                .Where(c => c.Id == id)
                .SelectMany(c => c.Courses)
                .Select(c => c.CourseId)
                .AsEnumerable();

            var candidates = await this.db
                .Certificates
                .Where(c => curriculumCourses.Contains(c.CourseId)) // curriculum certificates
                .Where(c => c.Student
                    .Certificates
                    .Where(cert => curriculumCourses.Contains(cert.CourseId))
                    .Select(cert => cert.CourseId)
                    .Distinct()
                    .Count() == curriculumCourses.Count()) // all certificates
                .Select(c => c.Student)
                .Distinct()
                .Where(c => !c.Diplomas.Any(d => d.CurriculumId == id))
                .OrderBy(u => u.Name) // without diploma
                .ProjectTo<AdminUserListingServiceModel>(this.mapper.ConfigurationProvider)
                .ToListAsync();

            return candidates;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var curriculum = this.db.Curriculums.Find(id);
            if (curriculum == null)
            {
                return false;
            }

            this.db.Curriculums.Remove(curriculum);
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> RemoveCourseAsync(int curriculumId, int courseId)
        {
            var curriculumCourse = await this.db.FindAsync<CurriculumCourse>(curriculumId, courseId);
            if (curriculumCourse == null)
            {
                return false;
            }

            this.db.Remove(curriculumCourse);
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdateAsync(int id, string name, string description)
        {
            var curriculum = this.db.Curriculums.Find(id);
            if (curriculum == null
                || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(description))
            {
                return false;
            }

            var newName = name.Trim();
            var newDescription = description.Trim();
            var result = 0;

            if (curriculum.Name != newName
                || curriculum.Description != newDescription)
            {
                curriculum.Name = newName;
                curriculum.Description = newDescription;

                result = await this.db.SaveChangesAsync();
            }

            return result > 0;
        }
    }
}
