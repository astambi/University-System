namespace University.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;

    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ICourseService courseService;
        private readonly IOrderService orderService;
        private readonly IPdfService pdfService;
        private readonly UserManager<User> userManager;

        public OrdersController(
            ICourseService courseService,
            IOrderService orderService,
            IPdfService pdfService,
            UserManager<User> userManager)
        {
            this.courseService = courseService;
            this.orderService = orderService;
            this.pdfService = pdfService;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCourses();
            }

            var userOrders = await this.orderService.AllByUserAsync(userId);

            return this.View(userOrders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCourses();
            }

            var order = await this.orderService.GetByIdForUserAsync(id, userId);
            if (order == null)
            {
                this.TempData.AddErrorMessage(WebConstants.OrderNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            return this.View(order);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCourses();
            }

            var canDeleteOrder = await this.orderService.CanBeDeletedAsync(id, userId);
            if (!canDeleteOrder)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseEnrollmentCancellationErrorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var order = await this.orderService.GetByIdForUserAsync(id, userId);
            if (order == null)
            {
                this.TempData.AddErrorMessage(WebConstants.OrderNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            return this.View(order);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToCourses();
            }

            var canDeleteOrder = await this.orderService.CanBeDeletedAsync(id, userId);
            if (!canDeleteOrder)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseEnrollmentCancellationErrorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            // Unsubscribe user from all order courses
            var unsubscribeSuccess = await this.courseService.CancelUserEnrollmentInOrderCoursesAsync(id, userId);
            if (!unsubscribeSuccess)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseEnrollmentCancellationErrorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            // Delete order
            var orderDeleteSuccess = await this.orderService.RemoveAsync(id, userId);
            if (!orderDeleteSuccess)
            {
                this.TempData.AddErrorMessage(
                    WebConstants.OrderCoursesEnrollmentCancellationSuccessMsg
                    + Environment.NewLine
                    + WebConstants.OrderDeletedErrorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            this.TempData.AddSuccessMessage(
                WebConstants.OrderCoursesEnrollmentCancellationSuccessMsg
                + Environment.NewLine
                + WebConstants.OrderDeletedSuccessMsg);

            return this.RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [Route(nameof(OrdersController.Invoice) + "/" + WebConstants.WithId)]
        public async Task<IActionResult> Invoice(string id)
        {
            var invoice = await this.orderService.GetInvoiceAsync(id);
            if (invoice == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvoiceNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            return this.View(invoice);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route(nameof(OrdersController.Invoice) + "/" + WebConstants.WithId)]
        public IActionResult DownloadInvoice(string id)
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

            var pdf = this.pdfService.ConvertToPdf(downloadUrl);
            if (pdf == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvoiceNotFoundMsg);
                return this.RedirectToAction(nameof(OrdersController.Index));
            }

            return this.File(pdf, WebConstants.ApplicationPdf, $"Invoice {id.ToUpper()}.pdf");
        }

        private IActionResult RedirectToCourses()
            => this.RedirectToAction(nameof(CoursesController.Index), WebConstants.CoursesController);
    }
}