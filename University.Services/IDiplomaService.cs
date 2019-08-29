namespace University.Services
{
    using System.Threading.Tasks;
    using University.Services.Models.Diplomas;

    public interface IDiplomaService
    {
        Task<DiplomaServiceModel> GetByIdAsync(string id);
    }
}
