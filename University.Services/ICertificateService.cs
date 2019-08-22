namespace University.Services
{
    using System.Threading.Tasks;
    using University.Services.Models.Certificates;

    public interface ICertificateService
    {
        Task<bool> CreateAsync(string trainerId, int courseId, string studentId, decimal gradeBg);

        Task<CertificateServiceModel> DownloadAsync(string certificateId);

        bool IsGradeEligibleForCertificate(decimal? gradeBg);

        Task<bool> RemoveAsync(string id, string trainerId, int courseId);
    }
}
