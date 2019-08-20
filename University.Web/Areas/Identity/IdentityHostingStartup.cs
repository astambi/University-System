using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(University.Web.Areas.Identity.IdentityHostingStartup))]
namespace University.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
            => builder.ConfigureServices((context, services) => { });
    }
}