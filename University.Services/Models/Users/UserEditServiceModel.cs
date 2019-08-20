namespace University.Services.Models.Users
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using University.Common.Mapping;
    using University.Data.Models;

    public class UserEditServiceModel : IMapFrom<User>
    {
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }
    }
}
