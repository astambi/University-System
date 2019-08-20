namespace University.Tests.Services
{
    using University.Services.Implementations;
    using Xunit;

    public class HtmlServiceTest
    {
        [Theory]
        [InlineData(@"<script>alert('xss')</script><div onload=""alert('xss')""")]
        [InlineData(@"javascript:alert('xss')")]
        public void Sanitize(string htmlToEscape)
        {
            // Arrange
            var htmlService = new HtmlService();

            var rawHtml =
                @"<script>alert('xss')</script><div onload=""alert('xss')"""
                + @"style=""background-color: test"">Test<img src=""test.gif"""
                + @"style=""background-image: url(javascript:alert('xss'));"
                + @"margin: 10px""></div>";

            var expectedContent =
                @"<div style=""background-color: test"">"
                + @"Test<img src="""
                + @"test.gif"""
                + @" style=""margin: 10px""></div>";

            // Act
            var resultContent = htmlService.Sanitize(rawHtml);

            // Assert
            Assert.DoesNotContain(htmlToEscape, resultContent);
            Assert.Equal(expectedContent, resultContent);
        }
    }
}
