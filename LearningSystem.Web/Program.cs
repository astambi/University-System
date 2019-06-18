namespace LearningSystem.Web
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        public static void Main(string[] args)
            => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
            => WebHost
            .CreateDefaultBuilder(args) // calls AddUserSecrets in Development environment
            .UseStartup<Startup>();
    }
}
