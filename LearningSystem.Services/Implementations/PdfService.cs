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

            // create a new pdf document converting an url 
            //var doc = converter.ConvertUrl($"https://localhost:5001/users/certificate/{id}");
            var doc = converter.ConvertUrl(url);

            // set converter options
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.MarginLeft = 10;
            converter.Options.MarginRight = 10;
            converter.Options.MarginTop = 20;
            converter.Options.MarginBottom = 20;

            // save pdf document 
            var pdf = doc.Save();

            // close pdf document 
            doc.Close();

            // return resulted pdf bytes 
            return pdf;
        }
    }
}
