namespace LearningSystem.Web.Infrastructure.Extensions
{
    using System;
    using System.Threading.Tasks;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDatabaseMigration(this IApplicationBuilder app, string adminPassword)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                // Migrate Database
                serviceScope.ServiceProvider.GetService<LearningSystemDbContext>()
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
                WebConstants.BlogAuthorRole,
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
