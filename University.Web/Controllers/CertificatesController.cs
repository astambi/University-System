namespace University.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using University.Services;
    using University.Web.Infrastructure.Extensions;

    [AllowAnonymous]
    public class CertificatesController : Controller
    {
        private const string CertificateDownloadPath = WebConstants.CertificatesController + "/" + WebConstants.WithId;
        private const string DiplomaDownloadPath = WebConstants.DiplomasController + "/" + WebConstants.WithId;

        private readonly ICertificateService certificateService;
        private readonly IDiplomaService diplomaService;
        private readonly IPdfService pdfService;

        public CertificatesController(
            ICertificateService certificateService,
            IDiplomaService diplomaService,
            IPdfService pdfService)
        {
            this.certificateService = certificateService;
            this.diplomaService = diplomaService;
            this.pdfService = pdfService;
        }

        [Route(CertificateDownloadPath)]
        public async Task<IActionResult> Certificate(string id) // read by pdfConverter
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

        [HttpPost]
        [Route(CertificateDownloadPath)]
        public IActionResult CertificateDownload(string id)
            => this.GeneratePdfFile(WebConstants.CertificateFileName);

        [Route(DiplomaDownloadPath)]
        public async Task<IActionResult> Diploma(string id) // read by pdfConverter
        {
            var diploma = await this.diplomaService.GetByIdAsync(id);
            if (diploma == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CertificateNotFoundMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            diploma.DownloadUrl = this.HttpContext.Request.GetRequestUrl();

            return this.View(diploma);
        }

        [HttpPost]
        [Route(DiplomaDownloadPath)]
        public IActionResult DiplomaDownload(string id)
            => this.GeneratePdfFile(WebConstants.DiplomaFileName);

        private IActionResult GeneratePdfFile(string fileName)
        {
            var downloadUrl = this.HttpContext.Request.GetRequestUrl();

            var pdf = this.pdfService.ConvertToPdf(downloadUrl);
            if (pdf == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CertificateNotFoundMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            return this.File(pdf, WebConstants.ApplicationPdf, fileName);
        }
    }
}