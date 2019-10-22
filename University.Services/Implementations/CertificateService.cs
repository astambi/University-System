namespace University.Services.Implementations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Data;
    using University.Data.Models;
    using University.Services.Models.Certificates;

    public class CertificateService : ICertificateService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public CertificateService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<bool> CreateAsync(string trainerId, int courseId, string studentId, decimal gradeBg)
        {
            var isCourseTrainer = await this.db
                .Courses
                .Where(c => c.Id == courseId)
                .Where(c => c.TrainerId == trainerId)
                .AnyAsync();

            var isUserEnrolledInCourse = await this.db
                .Courses
                .AnyAsync(c => c.Students.Any(sc => sc.StudentId == studentId));

            if (!(isCourseTrainer
                && isUserEnrolledInCourse
                && this.IsGradeEligibleForCertificate(gradeBg)))
            {
                return false;
            }

            var prevBestCertificate = await this.db
                .Certificates
                .Where(c => c.CourseId == courseId)
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.GradeBg) // best grade [2; 6]
                .FirstOrDefaultAsync();

            var canUpgradeCertificate =
                prevBestCertificate == null // no prev certificate
                || prevBestCertificate.GradeBg < gradeBg; // better grade

            if (!canUpgradeCertificate)
            {
                return false;
            }

            var certificate = new Certificate
            {
                Id = Guid.NewGuid().ToString().Replace("-", string.Empty),
                StudentId = studentId,
                CourseId = courseId,
                GradeBg = gradeBg,
                IssueDate = DateTime.UtcNow
            };

            await this.db.Certificates.AddAsync(certificate);
            await this.db.SaveChangesAsync();

            return true;
        }

        public async Task<CertificateServiceModel> DownloadAsync(string certificateId)
            => await this.db
            .Certificates
            .Where(c => c.Id == certificateId)
            .ProjectTo<CertificateServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public bool IsGradeEligibleForCertificate(decimal? gradeBg)
            => gradeBg != null
            && DataConstants.GradeBgCertificateMinValue <= gradeBg && gradeBg <= DataConstants.GradeBgMaxValue;

        public async Task<bool> RemoveAsync(string id, string trainerId, int courseId)
        {
            var certificate = this.db.Certificates.Find(id);

            var isCourseTrainer = await this.db
                .Courses
                .Where(c => c.Id == courseId)
                .Where(c => c.TrainerId == trainerId)
                .AnyAsync();

            if (certificate == null
                || !isCourseTrainer)
            {
                return false;
            }

            this.db.Certificates.Remove(certificate);
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }
    }
}
