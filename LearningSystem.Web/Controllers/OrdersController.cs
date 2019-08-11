namespace LearningSystem.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ICourseService courseService;
        private readonly IOrderService orderService;
        private readonly UserManager<User> userManager;

        public OrdersController(
            ICourseService courseService,
            IOrderService orderService,
            UserManager<User> userManager)
        {
            this.courseService = courseService;
            this.orderService = orderService;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RerirectToCourses();
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
                return this.RerirectToCourses();
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
                return this.RerirectToCourses();
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
                return this.RerirectToCourses();
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

        private IActionResult RerirectToCourses()
            => this.RedirectToAction(nameof(CoursesController.Index));
    }
}