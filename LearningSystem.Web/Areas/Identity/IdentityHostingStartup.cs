using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(LearningSystem.Web.Areas.Identity.IdentityHostingStartup))]
namespace LearningSystem.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
            => builder.ConfigureServices((context, services) => { });
    }
}