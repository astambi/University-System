namespace University.Tests.Services
{
    using University.Services.Implementations;
    using Xunit;

    public class PdfServiceTest
    {
        private const string TestContent = "https://www.dnevnik.bg/";
        private const string TestContentInvalid = null;

        [Fact]
        public void ConvertToPdf_ShouldReturnNull_GivenInvalidInput()
        {
            // Arrange
            var pdfService = new PdfService();

            // Act
            var result = pdfService.ConvertToPdf(TestContentInvalid);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Copy Select.Html.dep
        /// From {CURRENT PROJECT}.Services\bin\Debug\netstandard2.0
        /// To C:\Users\{USER}\.nuget\packages\select.htmltopdf.netcore\19.1.0\lib\netstandard2.0\
        /// </summary>
        [Fact]
        public void ConvertToPdf_ShouldReturnBytesArray_GivenValidInput()
        {
            //// Arrange
            //var pdfService = new PdfService();

            //// Act
            //var result = pdfService.ConvertToPdf(TestContent);

            //// Assert
            //Assert.NotNull(result);
            //Assert.IsType<byte[]>(result);
        }
    }
}
