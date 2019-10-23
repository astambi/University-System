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
                return this.RedirectToHomeIndex();
            }

            certificate.DownloadUrl = this.HttpContext.Request.GetRequestUrl();

            return this.View(certificate);
        }

        [HttpPost]
        [Route(CertificateDownloadPath)]
        public IActionResult CertificateDownload(string id)
            => this.GeneratePdfFile(nameof(Certificate));

        [Route(DiplomaDownloadPath)]
        public async Task<IActionResult> Diploma(string id) // read by pdfConverter
        {
            var diploma = await this.diplomaService.GetByIdAsync(id);
            if (diploma == null)
            {
                this.TempData.AddErrorMessage(WebConstants.DiplomaNotFoundMsg);
                return this.RedirectToHomeIndex();
            }

            diploma.DownloadUrl = this.HttpContext.Request.GetRequestUrl();

            return this.View(diploma);
        }

        [HttpPost]
        [Route(DiplomaDownloadPath)]
        public IActionResult DiplomaDownload(string id)
            => this.GeneratePdfFile(nameof(Diploma));

        private IActionResult GeneratePdfFile(string callingAction)
        {
            /// NB! SelectPdf Html To Pdf Converter for .NET – Community Edition (FREE) is used for the convertion of certificates & diplomas for the project.
            /// Community Edition works on Azure Web Apps, on Windows, starting with the Basic plan but it does NOT work with Free/Shared plans. 
            /// Therefore the option to convert a certificate / diploma / invoice with SelectPdf is disabled on for projects deployed on Azure. 
            /// Instead the user is redirected to the certificate / diploma view
            /// Read more about Deployment to Microsoft Azure here https://selectpdf.com/html-to-pdf/docs/html/Deployment-Microsoft-Azure.htm
            var downloadUrl = this.HttpContext.Request.GetRequestUrl();
            if (downloadUrl.Contains(WebConstants.AzureWeb))
            {
                return this.Redirect(downloadUrl);
            }

            return this.ConvertWithSelectPdf($"{callingAction}.pdf", downloadUrl);
        }

        private IActionResult ConvertWithSelectPdf(string fileName, string downloadUrl)
        {
            var pdf = this.pdfService.ConvertToPdf(downloadUrl);
            if (pdf == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CertificateNotFoundMsg);
                return this.RedirectToHomeIndex();
            }

            return this.File(pdf, WebConstants.ApplicationPdf, fileName);
        }

        private IActionResult RedirectToHomeIndex()
            => this.RedirectToAction(nameof(HomeController.Index), WebConstants.HomeController);
    }
}