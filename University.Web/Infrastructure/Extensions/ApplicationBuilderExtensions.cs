namespace University.Web.Infrastructure.Extensions
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using University.Data;
    using University.Data.Models;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDatabaseMigration(this IApplicationBuilder app, string adminPassword)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                // Migrate Database
                serviceScope.ServiceProvider.GetService<UniversityDbContext>()
                    .Database
                    .Migrate();

                // Seed Roles & Admin User
                Task
                    .Run(async () =>
                    {
                        await SeedRoles(serviceScope);
                        await SeedAdminUser(serviceScope, adminPassword);
                    })
                    .GetAwaiter()
                    .GetResult();
            }

            return app;
        }

        public static IApplicationBuilder UseEnvironmentOptions(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            return app;
        }

        public static IApplicationBuilder UseMvcRoutes(this IApplicationBuilder app)
        {
            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //      name: "areas",
            //      template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            //    );

            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});

            app
              .UseEndpoints(endpoints =>
              {
                  endpoints.MapAreaControllerRoute(
                      name: "admin",
                      areaName: "admin",
                      pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

                  endpoints.MapAreaControllerRoute(
                      name: "blog",
                      areaName: "blog",
                      pattern: "Blog/{controller=Home}/{action=Index}/{id?}");

                  endpoints.MapControllerRoute(
                      name: "default",
                      pattern: "{controller=Home}/{action=Index}/{id?}");

                  endpoints.MapRazorPages();
              });

            return app;
        }

        private static async Task SeedAdminUser(IServiceScope serviceScope, string adminPassword)
        {
            var userManager = serviceScope.ServiceProvider.GetService<UserManager<User>>();

            var adminUser = await userManager.FindByEmailAsync(WebConstants.AdminEmail);

            // Create Admin User
            if (adminUser == null)
            {
                adminUser = new User
                {
                    // IdentityUser
                    UserName = WebConstants.AdminUsername,
                    Email = WebConstants.AdminEmail,
                    // Custom User
                    Name = WebConstants.AdministratorRole,
                    Birthdate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
            }

            if (adminUser.Id != null)
            {
                var isInRoleAdmin = await userManager.IsInRoleAsync(adminUser, WebConstants.AdministratorRole);
                if (!isInRoleAdmin)
                {
                    await userManager.AddToRoleAsync(adminUser, WebConstants.AdministratorRole);
                }
            }
        }

        private static async Task SeedRoles(IServiceScope serviceScope)
        {
            var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

            var roles = new[]
            {
                WebConstants.AdministratorRole,
                WebConstants.BloggerRole,
                WebConstants.TrainerRole
            };

            foreach (var role in roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = role });
                }
            }
        }
    }
}
