namespace University.Tests.Mocks
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public static class ControllerContextMock
    {
        public static ControllerContext GetMock
            => new ControllerContext { HttpContext = new DefaultHttpContext() };

        public static ControllerContext HttpRequest(this ControllerContext controllerContext,
            string scheme, string host, string path)
        {
            var req = controllerContext.HttpContext.Request;

            req.Scheme = scheme;
            req.Host = new HostString(host);
            req.Path = new PathString(path);

            return controllerContext;
        }
    }
}
