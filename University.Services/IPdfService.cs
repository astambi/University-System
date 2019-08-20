namespace University.Services
{
    public interface IPdfService
    {
        byte[] ConvertToPdf(string url);
    }
}
