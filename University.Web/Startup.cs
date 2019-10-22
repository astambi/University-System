namespace University.Web
{
    using System;
    using AutoMapper;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using University.Data;
    using University.Data.Models;
    using University.Web.Infrastructure.Extensions;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            services
                .AddDbContext<UniversityDbContext>(options => options
                    .UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection")));

            services
                .AddIdentity<User, IdentityRole>() // App User
                                                   //.AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<UniversityDbContext>()
                .AddDefaultTokenProviders(); // Identity

            services.ConfigureIdentityOptions(); // Password & User settings

            // External Authentication providers
            services
                .AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = this.Configuration[WebConstants.AuthFacebookAppId];
                    options.AppSecret = this.Configuration[WebConstants.AuthFacebookAppSecret];
                })
                .AddGoogle(options =>
                {
                    options.ClientId = this.Configuration[WebConstants.AuthGoogleClientId];
                    options.ClientSecret = this.Configuration[WebConstants.AuthGoogleClientSecret];
                });

            // App Services
            services.AddDomainServices();
            services.AddCloudinaryService(
                this.Configuration[WebConstants.AuthCloudinaryCloudName],
                this.Configuration[WebConstants.AuthCloudinaryApiKey],
                this.Configuration[WebConstants.AuthCloudinaryApiSecret]);

            services.AddMemoryCache();

            services.AddSession();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            //services.AddAutoMapper(this.GetType());

            services.AddRoutingOptions(); // Lowercase URLs & query strings

            services.AddMvcOptions(); // Global filters & Identity

            services.ConfigureApplicationCookieOptions(); // Identity
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Database migration & seed
            app.UseDatabaseMigration(this.Configuration[WebConstants.AdminPassword]);

            app.UseEnvironmentOptions(env);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseMvcRoutes();
        }
    }
}
