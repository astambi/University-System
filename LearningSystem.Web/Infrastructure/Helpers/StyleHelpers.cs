namespace LearningSystem.Web.Infrastructure.Helpers
{
    using LearningSystem.Web.Models;

    public static class StyleHelpers
    {
        public static string ToStyle(string actionName)
            => WebConstants.Styles.ContainsKey(actionName)
            ? WebConstants.Styles[actionName]
            : WebConstants.PrimaryStyle;

        public static string ToStyle(FormActionEnum action)
            => ToStyle(action.ToString());
    }
}
