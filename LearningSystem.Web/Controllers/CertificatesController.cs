﻿namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    public class CertificatesController : Controller
    {
        private const string CertificateDownloadPath =
            WebConstants.CertificatesController + "/" + WebConstants.WithId;

        private readonly ICertificateService certificateService;
        private readonly IPdfService pdfService;

        public CertificatesController(
            ICertificateService certificateService,
            IPdfService pdfService)
        {
            this.certificateService = certificateService;
            this.pdfService = pdfService;
        }

        [AllowAnonymous]
        [Route(CertificateDownloadPath)]
        public async Task<IActionResult> Details(string id) // read by pdfConverter
        {
            var certificate = await this.certificateService.DownloadAsync(id);
            if (certificate == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CertificateNotFoundMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            certificate.DownloadUrl = this.HttpContext.Request.GetRequestUrl();

            return this.View(certificate);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route(CertificateDownloadPath)]
        public IActionResult Download(string id)
        {
            var downloadUrl = this.HttpContext.Request.GetRequestUrl();

            var pdf = this.pdfService.ConvertToPdf(downloadUrl);
            if (pdf == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CertificateNotFoundMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            return this.File(pdf, WebConstants.ApplicationPdf, WebConstants.CertificateFileName);
        }
    }
}