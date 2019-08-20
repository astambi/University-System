namespace University.Web.Infrastructure.Helpers
{
    using University.Web.Models;

    public static class StyleHelpers
    {
        public static string ToStyle(this string actionName)
            => WebConstants.Styles.ContainsKey(actionName)
            ? WebConstants.Styles[actionName]
            : WebConstants.OutlinePrimaryStyle;

        public static string ToStyle(this FormActionEnum action)
            => action.ToString().ToStyle();
    }
}
