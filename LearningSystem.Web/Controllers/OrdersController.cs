namespace LearningSystem.Web.Controllers
{
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
        private readonly IOrderService orderService;
        private readonly UserManager<User> userManager;

        public OrdersController(
            IOrderService orderService,
            UserManager<User> userManager)
        {
            this.orderService = orderService;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(CoursesController.Index));
            }

            var userOrders = await this.orderService.AllByUser(userId);

            return this.View(userOrders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(CoursesController.Index));
            }

            var order = await this.orderService.GetByIdForUser(id, userId);
            if (order == null)
            {
                this.TempData.AddErrorMessage(WebConstants.OrderNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            return this.View(order);
        }
    }
}