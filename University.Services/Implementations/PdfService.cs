namespace University.Services.Implementations
{
    using SelectPdf;

    public class PdfService : IPdfService
    {
        private readonly HtmlToPdf converter;

        public PdfService()
        {
            // Instantiate a html to pdf converter object
            this.converter = new HtmlToPdf();

            // Converter options
            this.converter.Options.PdfPageSize = PdfPageSize.A4;
            this.converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
        }

        public byte[] ConvertToPdf(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            // Create a new PDF document converting an URL
            var doc = this.converter.ConvertUrl(url);

            // Save PDF document 
            var pdf = doc.Save();

            // Close PDF document 
            doc.Close();

            // Return PDF bytes 
            return pdf;
        }
    }
}
