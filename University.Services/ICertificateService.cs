namespace University.Services
{
    using System.Threading.Tasks;
    using University.Data.Models;
    using University.Services.Models.Certificates;

    public interface ICertificateService
    {
        Task<bool> CreateAsync(string trainerId, int courseId, string studentId, Grade grade);

        Task<CertificateServiceModel> DownloadAsync(string certificateId);

        bool IsGradeEligibleForCertificate(Grade? grade);
    }
}
