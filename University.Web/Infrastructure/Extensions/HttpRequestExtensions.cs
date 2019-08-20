namespace University.Web.Infrastructure.Extensions
{
    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        public static string GetRequestUrl(this HttpRequest req)
            => $"{req.Scheme}://{req.Host.Value}{req.Path.Value}";
    }
}
