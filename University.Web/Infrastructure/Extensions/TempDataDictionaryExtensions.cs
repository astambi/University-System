namespace University.Web.Infrastructure.Extensions
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;

    public static class TempDataDictionaryExtensions
    {
        public static void AddInfoMessage(this ITempDataDictionary tempData, string message)
           => tempData[WebConstants.TempDataInfoMessageKey] = message;

        public static void AddSuccessMessage(this ITempDataDictionary tempData, string message)
            => tempData[WebConstants.TempDataSuccessMessageKey] = message;

        public static void AddErrorMessage(this ITempDataDictionary tempData, string message)
            => tempData[WebConstants.TempDataErrorMessageKey] = message;

        public static void AddErrorMessages(this ITempDataDictionary tempData, IdentityResult result)
        {
            if (result.Succeeded)
            {
                return;
            }

            tempData[WebConstants.TempDataErrorMessageKey] = string.Join(
                Environment.NewLine,
                result.Errors.Select(e => e.Description).ToList());
        }

        public static void AddInfoMessages(this ITempDataDictionary tempData, IdentityResult result)
        {
            if (result.Succeeded)
            {
                return;
            }

            tempData[WebConstants.TempDataInfoMessageKey] = string.Join(
                Environment.NewLine,
                result.Errors.Select(e => e.Description).ToList());
        }
    }
}
