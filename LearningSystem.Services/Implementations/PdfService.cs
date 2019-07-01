namespace LearningSystem.Services.Implementations
{
    using SelectPdf;

    public class PdfService : IPdfService
    {
        public byte[] ConvertToPdf(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            // instantiate a html to pdf converter object
            var converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            // create a new pdf document converting an url 
            var doc = converter.ConvertUrl(url);

            // save pdf document 
            var pdf = doc.Save();

            // close pdf document 
            doc.Close();

            // return pdf bytes 
            return pdf;
        }
    }
}
