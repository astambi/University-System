namespace University.Services.Models.Users
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class UserWithBirthdateServiceModel : UserServiceModel
    {
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }
    }
}
