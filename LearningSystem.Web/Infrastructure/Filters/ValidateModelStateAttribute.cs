namespace LearningSystem.Web.Infrastructure.Filters
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// This ActionFilter attribute validates the ModelState. 
    /// On invaid ModelState returns the View with the model.
    /// Use the action filter to replace:
    ///     if (!this.ModelState.IsValid) 
    ///     { 
    ///         return this.View(model); 
    ///     }
    /// NB!!! 
    /// Requirements: 
    ///     The calling Controller should inherit the MVC Controller 
    ///     & the Model name should contain "model"
    /// NB!!! 
    /// The action filter attribute should only be applied for validation of simple models!
    /// The action filter will not load the SelectListItems collection for the model. 
    /// If the model contains a collection of SelectListItems (a dropdown select option) 
    /// apply the standart ModelState validation. 
    /// </summary>
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var controller = context.Controller as Controller;
                if (controller == null)
                {
                    return;
                }

                var model = context.ActionArguments
                    .FirstOrDefault(a => a.Key.ToLower().Contains("model"))
                    .Value;
                if (model == null)
                {
                    return;
                }

                context.Result = controller.View(model);
            }
        }
    }
}
