namespace LearningSystem.Services
{
    public interface IPdfService
    {
        byte[] ConvertToPdf(string url);
    }
}
