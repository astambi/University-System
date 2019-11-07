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
    using University.Services.Models.EmailSender;
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
                .AddIdentity<User, IdentityRole>(options => options
                    .SignIn.RequireConfirmedAccount = false) // App User
                                                             //.AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<UniversityDbContext>()
                .AddDefaultTokenProviders(); // Identity

            services.ConfigureIdentityOptions(); // Password & User settings
            services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(3));

            // External Authentication providers
            services
                .AddAuthentication()
                .AddFacebook(options =>
                {
                    /// Facebook AppId & AppSecret intentionally removed from appsettings => add credentials to secrets.json
                    options.AppId = this.Configuration[WebConstants.AuthFacebookAppId];
                    options.AppSecret = this.Configuration[WebConstants.AuthFacebookAppSecret];
                })
                .AddGoogle(options =>
                {
                    /// Google ClientId & ClientSecret intentionally removed from appsettings => add credentials to secrets.json
                    options.ClientId = this.Configuration[WebConstants.AuthGoogleClientId];
                    options.ClientSecret = this.Configuration[WebConstants.AuthGoogleClientSecret];
                });

            // App Services
            services.AddDomainServices();

            /// Cloudinary ApiKey & ApiSecret intentionally removed from appsettings => add credentials to secrets.json
            services.AddCloudinaryService(
                this.Configuration[WebConstants.AuthCloudinaryCloudName],
                this.Configuration[WebConstants.AuthCloudinaryApiKey],
                this.Configuration[WebConstants.AuthCloudinaryApiSecret]);

            // Email Sender
            /// SendGrid ApiKey intentionally removed from appsettings => add credentials to secrets.json
            services.Configure<EmailSenderOptions>(this.Configuration.GetSection("Authentication:SendGrid"));

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
