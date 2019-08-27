namespace University.Tests.Services.Admin
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using University.Data;
    using University.Data.Models;
    using University.Services.Admin;
    using University.Services.Admin.Implementations;
    using University.Services.Admin.Models.Users;
    using Xunit;

    public class AdminUserServiceTest
    {
        [Fact]
        public async Task AllAsync_ShouldReturnCorrectDataAndOrder()
        {
            // Arrange
            var db = await this.PrepareUsers();
            var adminUserService = this.InitializeAdminUserService(db);

            var expected = db
                .Users
                .OrderBy(u => u.UserName)
                .ToList();

            var expectedIdOrder = new List<string> { "2", "1", "3" };

            // Act
            var result = await adminUserService.AllAsync();
            var resultList = result.ToList();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<AdminUserListingServiceModel>>(result);

            Assert.Equal(expected.Count, resultList.Count);
            Assert.Equal(expectedIdOrder, result.Select(u => u.Id).ToList());

            for (var i = 0; i < resultList.Count; i++)
            {
                var expectedUser = expected[i];
                var resultUser = resultList[i];

                Assert.Equal(expectedIdOrder[i], resultUser.Id);
                this.AssertUser(expectedUser, resultUser);
            }
        }

        private void AssertUser(User expectedUser, AdminUserListingServiceModel resultUser)
        {
            Assert.Equal(expectedUser.Id, resultUser.Id);
            Assert.Equal(expectedUser.Name, resultUser.Name);
            Assert.Equal(expectedUser.UserName, resultUser.Username);
            Assert.Equal(expectedUser.Email, resultUser.Email);
        }

        private async Task<UniversityDbContext> PrepareUsers()
        {
            var user1 = new User { Id = "1", Name = "Name 1", UserName = "UsernameBBBBB", Email = "email.1@gmail.com" };
            var user2 = new User { Id = "2", Name = "Name 2", UserName = "UsernameAAAAA", Email = "email.2@gmail.com" };
            var user3 = new User { Id = "3", Name = "Name 3", UserName = "UsernameCCCCC", Email = "email.3@gmail.com" };

            var db = Tests.InitializeDatabase();
            await db.Users.AddRangeAsync(user1, user2, user3);
            await db.SaveChangesAsync();

            return db;
        }

        private IAdminUserService InitializeAdminUserService(UniversityDbContext db)
            => new AdminUserService(db, Tests.Mapper);
    }
}
