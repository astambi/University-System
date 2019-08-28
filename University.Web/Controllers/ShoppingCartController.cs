namespace University.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Models.ShoppingCart;

    public class ShoppingCartController : Controller
    {
        private readonly ICourseService courseService;
        private readonly IOrderService orderService;
        private readonly IShoppingCartManager shoppingCartManager;
        private readonly UserManager<User> userManager;

        public ShoppingCartController(
            ICourseService courseService,
            IOrderService orderService,
            IShoppingCartManager shoppingCartManager,
            UserManager<User> userManager)
        {
            this.courseService = courseService;
            this.orderService = orderService;
            this.shoppingCartManager = shoppingCartManager;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var shoppingCartId = this.GetShoppingCartId();
            var cartItems = this.shoppingCartManager.GetCartItems(shoppingCartId);

            var userId = this.userManager.GetUserId(this.User);

            var cartItemsWithDetails = await this.courseService.GetCartItemsDetailsForUserAsync(cartItems, userId);
            if (!cartItemsWithDetails.Any())
            {
                this.shoppingCartManager.EmptyCart(shoppingCartId);
            }

            return this.View(cartItemsWithDetails);
        }

        public async Task<IActionResult> Add(int id)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToCourses();
            }

            var userId = this.userManager.GetUserId(this.User);
            var isUserEnrolled = userId != null
                && await this.courseService.IsUserEnrolledInCourseAsync(id, userId);
            if (isUserEnrolled)
            {
                this.TempData.AddInfoMessage(WebConstants.UserEnrolledInCourseAlreadyMsg);
                return this.RedirectToCourseDetails(id);
            }

            var shoppingCartId = this.GetShoppingCartId();
            this.shoppingCartManager.AddItemToCart(shoppingCartId, id);
            this.TempData.AddSuccessMessage(WebConstants.CourseAddedToShoppingCartSuccessMsg);

            return this.RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutFormModel model)
        {
            // Validations
            if (!this.ModelState.IsValid)
            {
                this.TempData.AddErrorMessage(WebConstants.PaymentMethodInvalidMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var shoppingCartId = this.GetShoppingCartId();
            var cartItems = this.shoppingCartManager.GetCartItems(shoppingCartId);
            if (!cartItems.Any())
            {
                this.TempData.AddInfoMessage(WebConstants.ShoppingCartEmptyMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var cartItemsWithDetails = await this.courseService.GetCartItemsDetailsForUserAsync(cartItems, userId);
            if (!cartItemsWithDetails.Any())
            {
                this.shoppingCartManager.EmptyCart(shoppingCartId); // empty any remaining items in cart
                this.TempData.AddInfoMessage(WebConstants.ShoppingCartEmptyMsg);
                return this.RedirectToAction(nameof(Index));
            }

            // Create order with payment
            var totalPrice = cartItemsWithDetails.Sum(i => i.Price);
            if (totalPrice != model.TotalPrice)
            {
                this.TempData.AddInfoMessage(WebConstants.ShoppingCartItemsMismatchMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var orderId = await this.orderService.CreateAsync(userId, model.PaymentMethod.Value, totalPrice, cartItemsWithDetails);
            if (orderId < 0)
            {
                this.TempData.AddErrorMessage(WebConstants.PaymentErrorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            this.TempData.AddSuccessMessage(WebConstants.OrderCreatedSuccessMsg);
            this.shoppingCartManager.EmptyCart(shoppingCartId);

            await this.EnrollUserInOrderCourses(userId, orderId);

            return this.RedirectToOrderDetails(orderId);
        }

        public IActionResult Clear()
        {
            var shoppingCartId = this.GetShoppingCartId();
            this.shoppingCartManager.EmptyCart(shoppingCartId);

            this.TempData.AddSuccessMessage(WebConstants.ShoppingCartClearedMsg);

            return this.RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int id)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToCourses();
            }

            var shoppingCartId = this.GetShoppingCartId();
            this.shoppingCartManager.RemoveItemFromCart(shoppingCartId, id);

            this.TempData.AddSuccessMessage(WebConstants.CourseRemovedFromShoppingCartSuccessMsg);

            return this.RedirectToAction(nameof(Index));
        }

        private async Task EnrollUserInOrderCourses(string userId, int orderId)
        {
            var success = await this.courseService.EnrollUserInOrderCoursesAsync(orderId, userId);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseEnrollmentErrorMsg);
            }
        }

        private string GetShoppingCartId()
            => this.HttpContext.Session.GetOrSetShoppingCartId();

        private IActionResult RedirectToCourseDetails(int id)
           => this.RedirectToAction(nameof(CoursesController.Details), WebConstants.CoursesController, new { id });

        private IActionResult RedirectToCourses()
            => this.RedirectToAction(nameof(CoursesController.Index), WebConstants.CoursesController);

        private IActionResult RedirectToOrderDetails(int orderId)
            => this.RedirectToAction(nameof(OrdersController.Details), WebConstants.OrdersController, new { id = orderId });
    }
}