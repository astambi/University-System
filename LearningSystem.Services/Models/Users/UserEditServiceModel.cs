namespace LearningSystem.Services.Models.Users
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class UserEditServiceModel : IMapFrom<User>
    {
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }
    }
}
