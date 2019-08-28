namespace University.Tests.Mocks
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Moq;
    using University.Web;
    using Xunit;

    public static class TempDataMock
    {
        public static ITempDataDictionary GetMock
            => new TempDataDictionary(
                context: new DefaultHttpContext(),
                provider: Mock.Of<ITempDataProvider>());

        public static void AssertErrorMsg(this ITempDataDictionary tempData, string errorMsg)
        {
            AssertErrorKey(tempData);
            Assert.Equal(errorMsg, tempData[WebConstants.TempDataErrorMessageKey]);
        }

        public static void AssertSuccessMsg(this ITempDataDictionary tempData, string errorMsg)
        {
            AssertSuccessKey(tempData);
            Assert.Equal(errorMsg, tempData[WebConstants.TempDataSuccessMessageKey]);
        }

        private static void AssertErrorKey(this ITempDataDictionary tempData)
            => tempData.AssertKey(WebConstants.TempDataErrorMessageKey);

        private static void AssertSuccessKey(this ITempDataDictionary tempData)
            => tempData.AssertKey(WebConstants.TempDataSuccessMessageKey);

        private static void AssertKey(this ITempDataDictionary tempData, string key)
           => Assert.Contains(tempData.Keys, k => k == key);
    }
}
