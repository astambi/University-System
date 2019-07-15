namespace LearningSystem.Tests.Mocks
{
    using System.Collections.Generic;
    using LearningSystem.Data.Models;
    using LearningSystem.Web;
    using Microsoft.AspNetCore.Identity;
    using Moq;

    public class UserManagerMock
    {
        public static Mock<UserManager<User>> GetMock
           => new Mock<UserManager<User>>(
               Mock.Of<IUserStore<User>>(),
               null, null, null, null, null, null, null, null);

        public static void GetUsersInRoleAsync(Mock<UserManager<User>> mock, IList<User> trainers)
            => mock.Setup(u => u.GetUsersInRoleAsync(WebConstants.TrainerRole))
            .ReturnsAsync(trainers)
            .Verifiable();
    }
}
