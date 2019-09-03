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
        private const string CertificateName = "Certificate.pdf";

        private const string Scheme = "https";
        private const string Host = "mysite.com";
        private const string Path = "/certificates/" + CertificateValidId;
        private const string DownloadUrl = Scheme + "://" + Host + Path;
        private const string DownloadUrlAzure = Scheme + "://" + WebConstants.AzureWeb + Path;

        // Attributes
        [Fact]
        public void CertificatesController_ShouldBeAnonymousUsers()
        {
            // Arrange
            var attributes = typeof(CertificatesController).GetCustomAttributes(true);

            // Act
            var allowAnonymousAttribute = attributes
                .FirstOrDefault(a => a.GetType() == typeof(AllowAnonymousAttribute))
                as AllowAnonymousAttribute;

            // Assert
            Assert.NotNull(allowAnonymousAttribute);
        }

        [Fact]
        public void Certificate_ShouldHaveCorrectRouteAttribute()
            => this.AssertAttributeRouteWithId(nameof(CertificatesController.Certificate));

        [Fact]
        public async Task Certificate_ShouldRedirectToHome_GivenInvalidCertificate()
        {
            // Arrange
            var certificateService = CertificateServiceMock.GetMock;
            certificateService.DownloadAsync(null);

            var controller = new CertificatesController(
                certificateService.Object,
                diplomaService: null,
                pdfService: null)
            {
                TempData = TempDataMock.GetMock
            };

            // Act
            var result = await controller.Certificate(CertificateInvalidId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CertificateNotFoundMsg);

            this.AssertRedirectToHomeControllerIndex(result);

            certificateService.Verify();
        }

        [Fact]
        public async Task Certificate_ShouldReturnViewResultWithCorrectModel_GivenValidCertificate()
        {
            // Arrange
            var certificateService = CertificateServiceMock.GetMock;
            certificateService.DownloadAsync(this.GetCertificate());

            var controller = new CertificatesController(
                certificateService.Object,
                diplomaService: null,
                pdfService: null)
            {
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(Scheme, Host, Path); // HttpRequest Mock

            // Act
            var result = await controller.Certificate(CertificateValidId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CertificateServiceModel>(viewResult.Model);

            this.AsserCertificate(model);

            certificateService.Verify();
        }

        [Fact]
        public void CertificateDownload_ShouldHaveCorrectRouteAttribute()
            => this.AssertAttributeRouteWithId(nameof(CertificatesController.CertificateDownload));

        [Fact]
        public void CertificateDownload_ShouldHaveHttpPostAttribute()
        {
            // Arrange
            var method = typeof(CertificatesController)
                .GetMethod(nameof(CertificatesController.CertificateDownload));

            // Act
            var attributes = method.GetCustomAttributes(true);

            // Assert
            Assert.Contains(attributes, a => a.GetType() == typeof(HttpPostAttribute));
        }

        [Fact]
        public void CertificateDownload_ShouldRedirectToHome_GivenInvalidPath()
        {
            // Arrange
            var pdfService = PdfServiceMock.GetMock;
            pdfService.ConvertToPdf(null);

            var controller = new CertificatesController(
                certificateService: null,
                diplomaService: null,
                pdfService.Object)
            {
                TempData = TempDataMock.GetMock,
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(Scheme, Host, Path); // HttpRequest Mock

            // Act
            var result = controller.CertificateDownload(CertificateValidId);

            // Assert
            controller.TempData.AssertErrorMsg(WebConstants.CertificateNotFoundMsg);

            this.AssertRedirectToHomeControllerIndex(result);

            pdfService.Verify();
        }

        [Fact]
        public void CertificateDownload_ShouldRedirectToView_GivenAzureDeployment()
        {
            // Arrange
            var controller = new CertificatesController(
                certificateService: null,
                diplomaService: null,
                pdfService: null)
            {
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(Scheme, WebConstants.AzureWeb, Path); // HttpRequest Mock

            // Act
            var result = controller.CertificateDownload(CertificateValidId);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(DownloadUrlAzure, redirectResult.Url);
        }

        [Fact]
        public void CertificateDownload_ShouldReturnFileContentResultWithCorrectContent_GivenServiceSuccess()
        {
            // Arrange
            var pdfService = PdfServiceMock.GetMock;
            pdfService.ConvertToPdf(this.GetCertificateFileBytes());

            var controller = new CertificatesController(
                certificateService: null,
                diplomaService: null,
                pdfService.Object)
            {
                ControllerContext = ControllerContextMock.GetMock // HttpRequest Mock
            };
            controller.ControllerContext.HttpRequest(Scheme, Host, Path); // HttpRequest Mock

            // Act
            var result = controller.CertificateDownload(CertificateValidId);

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
            Assert.Equal(CertificateName, fileContentResult.FileDownloadName);
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
