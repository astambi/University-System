namespace University.Tests.Mocks
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using University.Data.Models;
    using University.Web;

    public static class UserManagerMock
    {
        public static Mock<UserManager<User>> GetMock
           => new Mock<UserManager<User>>(
               Mock.Of<IUserStore<User>>(),
               null, null, null, null, null, null, null, null);

        public static Mock<UserManager<User>> FindByIdAsync(this Mock<UserManager<User>> mock, User user)
        {
            mock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user)
                .Verifiable();

            return mock;
        }

        public static Mock<UserManager<User>> GetRolesAsync(this Mock<UserManager<User>> mock, IList<string> roles)
        {
            mock.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
            //.Callback((User userParam) => userInput = userParam) // service input
            .ReturnsAsync(roles)
            .Verifiable();
            return mock;
        }

        public static Mock<UserManager<User>> GetUserAsync(this Mock<UserManager<User>> mock, User user)
        {
            mock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user)
                .Verifiable();
            return mock;
        }

        public static Mock<UserManager<User>> GetUserId(this Mock<UserManager<User>> mock, string userId)
        {
            mock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId)
                .Verifiable();
            return mock;
        }

        public static Mock<UserManager<User>> GetUsersInRoleAsync(this Mock<UserManager<User>> mock, string role, IList<User> trainers)
        {
            mock.Setup(u => u.GetUsersInRoleAsync(role))
                .ReturnsAsync(trainers)
                .Verifiable();
            return mock;
        }

        public static Mock<UserManager<User>> GetUsersInRoleTrainerAsync(this Mock<UserManager<User>> mock, IList<User> trainers)
            => GetUsersInRoleAsync(mock, WebConstants.TrainerRole, trainers);
    }
}
