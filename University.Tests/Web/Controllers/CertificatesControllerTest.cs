namespace University.Tests.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using University.Data;
    using University.Services.Models.Certificates;
    using University.Tests.Mocks;
    using University.Web;
    using University.Web.Controllers;
    using Xunit;

    public class CertificatesControllerTest
    {
        private const string CertificateValidId = "CertificateValidId";
        private const string CertificateInvalidId = "CertificateInvalidId";

        private const string Scheme = "https";
        private const string Host = "mysite.com";
        private const string Path = "/certificates/" + CertificateValidId;
        private const string DownloadUrl = Scheme + "://" + Host + Path;

        [Fact]
        public void Details_ShouldAllowAnonymousUsers()
            => this.AssertAttributeAllowAnonymous(nameof(CertificatesController.Details));

        [Fact]
        public void Details_ShouldHaveCorrectRouteAttribute()
            => this.AssertAttributeRouteWithId(nameof(CertificatesController.Details));

        [Fact]
        public async Task Details_ShouldRedirectToHome_GivenInvalidCertificate()
        {
            // Arrange
            var certificateService = CertificateServiceMock.GetMock;
            certificateService.DownloadAsync(null);

            var controller = new CertificatesController(
                certificateService.Object,
                pdfService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Details(CertificateInvalidId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CertificateNotFoundMsg);

            this.AssertRedirectToHomeControllerIndex(result);

            certificateService.Verify();
        }

        [Fact]
        public async Task Details_ShouldReturnViewResultWithCorrectModel_GivenValidCertificate()
        {
            // Arrange
            var certificateService = CertificateServiceMock.GetMock;
            certificateService.DownloadAsync(this.GetCertificate());

            var controller = new CertificatesController(
                certificateService.Object,
                pdfService: null)
            {
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(Scheme, Host, Path); // HttpRequest Mock

            // Act
            var result = await controller.Details(CertificateValidId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CertificateServiceModel>(viewResult.Model);

            this.AsserCertificate(model);

            certificateService.Verify();
        }

        [Fact]
        public void Download_ShouldAllowAnonymousUsers()
            => this.AssertAttributeAllowAnonymous(nameof(CertificatesController.Download));

        [Fact]
        public void Download_ShouldHaveCorrectRouteAttribute()
            => this.AssertAttributeRouteWithId(nameof(CertificatesController.Download));

        [Fact]
        public void Download_ShouldHaveHttpPostAttribute()
        {
            // Arrange
            var method = typeof(CertificatesController)
                .GetMethod(nameof(CertificatesController.Download));

            // Act
            var attributes = method.GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(HttpPostAttribute));
        }

        [Fact]
        public void Download_ShouldRedirectToHome_GivenInvalidPath()
        {
            // Arrange
            var pdfService = PdfServiceMock.GetMock;
            pdfService.ConvertToPdf(null);

            var controller = new CertificatesController(
                certificateService: null,
                pdfService.Object)
            {
                TempData = TempDataMock.GetMock,
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(Scheme, Host, Path); // HttpRequest Mock

            // Act
            var result = controller.Download(CertificateValidId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CertificateNotFoundMsg);

            this.AssertRedirectToHomeControllerIndex(result);

            pdfService.Verify();
        }

        [Fact]
        public void Download_ShouldReturnFileContentResultWithCorrectContent_GivenServiceSuccess()
        {
            // Arrange
            var pdfService = PdfServiceMock.GetMock;
            pdfService.ConvertToPdf(this.GetCertificateFileBytes());

            var controller = new CertificatesController(
                certificateService: null,
                pdfService.Object)
            {
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(Scheme, Host, Path); // HttpRequest Mock

            // Act
            var result = controller.Download(CertificateValidId);

            // Assert
            var fileContentResult = Assert.IsType<FileContentResult>(result);
            this.AssertCertificateFileContent(fileContentResult);

            pdfService.Verify();
        }

        private void AssertAttributeAllowAnonymous(string methodName)
        {
            // Arrange
            var method = typeof(CertificatesController).GetMethod(methodName);

            // Act
            var attributes = method.GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(AllowAnonymousAttribute));
        }

        private void AssertAttributeRouteWithId(string methodName)
        {
            // Arrange
            var method = typeof(CertificatesController).GetMethod(methodName);

            // Act
            var routeAttribute = method
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            // Assert
            Assert.NotNull(routeAttribute);
            Assert.Equal(
                WebConstants.CertificatesController + "/" + WebConstants.WithId,
                routeAttribute.Template);
        }

        private void AsserCertificate(CertificateServiceModel certificate)
        {
            var expectedCertificate = this.GetCertificate();

            Assert.NotNull(certificate);

            Assert.Equal(expectedCertificate.Id, certificate.Id);
            Assert.Equal(expectedCertificate.StudentName, certificate.StudentName);
            Assert.Equal(expectedCertificate.CourseName, certificate.CourseName);
            Assert.Equal(expectedCertificate.CourseStartDate, certificate.CourseStartDate);
            Assert.Equal(expectedCertificate.CourseEndDate, certificate.CourseEndDate);
            Assert.Equal(expectedCertificate.CourseTrainerName, certificate.CourseTrainerName);
            Assert.Equal(expectedCertificate.GradeBg, certificate.GradeBg);
            Assert.Equal(expectedCertificate.IssueDate, certificate.IssueDate);
            Assert.Equal(expectedCertificate.DownloadUrl, certificate.DownloadUrl);
        }

        private void AssertCertificateFileContent(FileContentResult fileContentResult)
        {
            Assert.Equal(this.GetCertificateFileBytes(), fileContentResult.FileContents);
            Assert.Equal(WebConstants.ApplicationPdf, fileContentResult.ContentType);
            Assert.Equal(WebConstants.CertificateFileName, fileContentResult.FileDownloadName);
        }

        private void AssertRedirectToHomeControllerIndex(IActionResult result)
        {
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(HomeController.Index), redirectToActionResult.ActionName);
        }

        private CertificateServiceModel GetCertificate()
           => new CertificateServiceModel
           {
               Id = CertificateValidId,
               StudentName = "Student",
               CourseName = "Course",
               CourseStartDate = new DateTime(2019, 1, 1),
               CourseEndDate = new DateTime(2019, 5, 15),
               GradeBg = DataConstants.GradeBgMaxValue,
               CourseTrainerName = "TrainerId",
               IssueDate = new DateTime(2019, 7, 10),
               DownloadUrl = DownloadUrl
           };

        private byte[] GetCertificateFileBytes()
            => new byte[] { 101, 1, 27, 8, 11, 17, 57 };
    }
}
