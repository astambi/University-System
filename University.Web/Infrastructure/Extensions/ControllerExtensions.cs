namespace University.Web.Infrastructure.Extensions
{
    using Microsoft.AspNetCore.Mvc;

    public static class ControllerExtensions
    {
        public static IActionResult ViewOrNotFound(this Controller controller, object model)
        {
            if (model == null)
            {
                return controller.NotFound("Content not found");
            }

            return controller.View(model);
        }

        public static IActionResult ViewOrRedirect(this Controller controller, object model)
        {
            if (model == null)
            {
                return controller.Redirect("/");
            }

            return controller.View(model);
        }
    }
}
